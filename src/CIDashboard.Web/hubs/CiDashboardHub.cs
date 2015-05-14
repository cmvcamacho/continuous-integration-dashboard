using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CIDashboard.Web.Infrastructure.Interfaces;
using CIDashboard.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Serilog;

namespace CIDashboard.Web.Hubs
{
    [Authorize]
    [HubName("ciDashboardHub")]
    public class CiDashboardHub : Hub<ICiDashboardHub>
    {
        private static readonly ILogger Logger = Log.ForContext<CiDashboardHub>();
        private readonly IConnectionsManager connectionsManager;
        private readonly IInformationQuery infoQuery;
        private readonly IRefreshInformation refreshInfo;
        private readonly ICommandProcessor commandProcessor;

        public CiDashboardHub(IConnectionsManager connectionsManager, ICommandProcessor commandProcessor, IInformationQuery infoQuery, IRefreshInformation refreshInfo)
        {
            this.connectionsManager = connectionsManager;
            this.commandProcessor = commandProcessor;
            this.infoQuery = infoQuery;
            this.refreshInfo = refreshInfo;
        }

        public override async Task OnConnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnConnected for {username} and {connectionId}", username, connectionId);

            await this.GetUserProjectsAndBuildsAndSendTheInfoToClient(username, connectionId);
            await this.refreshInfo.SendRefreshBuildResults(connectionId);

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            Logger.Debug("OnDisconnected for {connectionId}", connectionId);

            await this.connectionsManager.RemoveAllBuildConfigs(connectionId);

            await base.OnDisconnected(stopCalled);
        }

        public override async Task OnReconnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnReconnected for {username} and {connectionId}", username, connectionId);

            await this.GetUserProjectsAndBuildsAndSendTheInfoToClient(username, connectionId);

            await base.OnReconnected();
        }

        public async Task RequestRefresh()
        {
            var connectionId = Context.ConnectionId;
            await this.refreshInfo.SendRefreshBuildResults(connectionId);
        }

        public async Task RequestAllProjectBuilds()
        {
            var connectionId = Context.ConnectionId;
            await this.RequestAllProjectBuilds(connectionId);
        }

        public async Task AddNewProject(Project project)
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;
            var projectCreated = await this.commandProcessor.AddNewProject(username, project);

            if (projectCreated != null)
                this.Clients.Client(connectionId)
                    .SendUpdatedProject(new ProjectUpdated { OldId = project.Id, Project = projectCreated });
        }

        public async Task UpdateProjectName(int projectId, string projectName)
        {
            var connectionId = Context.ConnectionId;
            var updated = await this.commandProcessor.UpdateProjectName(projectId, projectName);
            if (updated)
                await SendSuccessMessage(connectionId, string.Format("Project {0} updated.", projectName));
        }

        public async Task RemoveProject(int projectId)
        {
            var connectionId = Context.ConnectionId;
            var projectRemoved = await this.commandProcessor.RemoveProject(projectId);
            if(projectRemoved != null)
            {
                foreach(var buildConfig in projectRemoved.Builds)
                {
                    await connectionsManager.RemoveBuildConfig(connectionId, buildConfig);
                }
                await SendSuccessMessage(connectionId, "Project removed");
            }
        }

        public async Task AddBuildToProject(int projectId, BuildConfig build)
        {
            var connectionId = Context.ConnectionId;
            var buildCreated = await this.commandProcessor.AddBuildConfigToProject(projectId, build);
            if (buildCreated != null)
            {
                await connectionsManager.UpdateBuildConfigs(connectionId, new List<BuildConfig> { buildCreated });
                this.Clients.Client(connectionId)
                    .SendUpdatedBuild(new BuildConfigUpdated { OldId = build.Id, Build = buildCreated });
            }
        }

        public async Task RemoveBuild(int buildId)
        {
            var connectionId = Context.ConnectionId;
            var buildConfigRemoved = await this.commandProcessor.RemoveBuildConfig(buildId);
            if (buildConfigRemoved != null)
            {
                await connectionsManager.RemoveBuildConfig(connectionId, buildConfigRemoved);
                await SendSuccessMessage(connectionId, "Build removed");
            }
        }

        public async Task UpdateBuildConfigExternalId(int buildId, string buildName, string externalId)
        {
            var connectionId = Context.ConnectionId;
            var updated = await this.commandProcessor.UpdateBuildConfigExternalId(buildId, buildName, externalId);
            if(updated)
            {
                await connectionsManager.UpdateBuildConfigs(connectionId, new[] { new BuildConfig { CiExternalId = externalId } });
                await SendSuccessMessage(connectionId, string.Format("Build {0} updated.", buildName));
            }
        }

        private async Task GetUserProjectsAndBuildsAndSendTheInfoToClient(string username, string connectionId)
        {
            var userProjects = await this.infoQuery.GetUserProjectsAndBuildConfigs(username);
            var buildCiIds = userProjects
                .SelectMany(p => p.Builds
                    .Where(b => !string.IsNullOrEmpty(b.CiExternalId))
                    .ToList())
                .ToList();

            await this.connectionsManager.AddBuildConfigs(connectionId, buildCiIds);

            Logger.Debug("Start retrieving builds for {user} and {connectionId}", username, connectionId);

            this.Clients.Client(connectionId).SendProjectsAndBuildConfigs(userProjects);
            await SendInfoMessage(connectionId, "Your builds are being retrieved");
        }

        private async Task RequestAllProjectBuilds(string connectionId)
        {
            try
            {
                var allProjectBuilds = await this.infoQuery.GetAllProjectBuildConfigs();

                this.Clients.Client(connectionId).SendProjectBuilds(allProjectBuilds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error requesting all project builds...");
            }
        }

        private async Task SendInfoMessage(string connectionId, string message)
        {
            await Task.Run(() => this.Clients.Client(connectionId).SendMessage(new FeedbackMessage { Status = "Info", Message = message }));
        }

        private async Task SendSuccessMessage(string connectionId, string message)
        {
            await Task.Run(() => this.Clients.Client(connectionId).SendMessage(new FeedbackMessage { Status = "Success", Message = message }));
        }

        private async Task SendErrorMessage(string connectionId, string message)
        {
            await Task.Run(() => this.Clients.Client(connectionId).SendMessage(new FeedbackMessage { Status = "Error", Message = message }));
        }
    }
}