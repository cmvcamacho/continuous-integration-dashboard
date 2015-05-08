using System.Threading.Tasks;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Serilog;

namespace CIDashboard.Web.Hubs
{
    [Authorize]
    [HubName("ciDashboardHub")]
    public class CiDashboardHub : Hub
    {
        private static readonly ILogger Logger = Log.ForContext<CiDashboardHub>();
        private readonly IQueryController queryController;
        private readonly ICommandController commandController;

        public CiDashboardHub(IQueryController queryController, ICommandController commandController)
        {
            this.queryController = queryController;
            this.commandController = commandController;
        }

        public override async Task OnConnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnConnected for {username} and {connectionId}", username, connectionId);

            await this.queryController.AddBuilds(username, connectionId);

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            Logger.Debug("OnDisconnected for {connectionId}", connectionId);

            await this.queryController.RemoveBuilds(connectionId);

            await base.OnDisconnected(stopCalled);
        }

        public override async Task OnReconnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnReconnected for {username} and {connectionId}", username, connectionId);

            await this.queryController.AddBuilds(username, connectionId);

            await base.OnReconnected();
        }

        public async Task RequestRefresh()
        {
            var connectionId = Context.ConnectionId;
            await this.queryController.RefreshBuilds(connectionId);
        }

        public async Task RequestAllProjectBuilds()
        {
            var connectionId = Context.ConnectionId;
            await this.queryController.RequestAllProjectBuilds(connectionId);
        }

        public async Task AddNewProject(Project project)
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;
            var projectCreated = await this.commandController.AddNewProject(username, project);
            if (projectCreated != null)
                await this.queryController.UpdateProject(connectionId, project.Id, projectCreated);
        }

        public async Task UpdateProjectName(int projectId, string projectName)
        {
            var connectionId = Context.ConnectionId;
            var updated = await this.commandController.UpdateProjectName(projectId, projectName);
            if (updated)
                await this.queryController.SendMessage(connectionId, string.Format("Project {0} updated.", projectName));
        }

        public async Task RemoveProject(int projectId)
        {
            var connectionId = Context.ConnectionId;
            var removed = await this.commandController.RemoveProject(projectId);
            if (removed)
                await this.queryController.SendMessage(connectionId, "Project removed.");
        }

        public async Task AddBuildToProject(int projectId, Build build)
        {
            var connectionId = Context.ConnectionId;
            var buildCreated = await this.commandController.AddBuildToProject(projectId, build);
            if (buildCreated != null)
            {
                await this.queryController.UpdateBuild(connectionId, build.Id, buildCreated);
            }
        }
    }
}