using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Entities;
using CIDashboard.Domain.Extensions;
using CIDashboard.Domain.Services;
using CIDashboard.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Serilog;
using Build = CIDashboard.Web.Models.Build;

namespace CIDashboard.Web.Infrastructure
{
    public class QueryController : IQueryController
    {
        internal static ConcurrentDictionary<string, List<string>> BuildsPerConnId = new ConcurrentDictionary<string, List<string>>();
        internal static ConcurrentDictionary<string, string> BuildsToBeRefreshed = new ConcurrentDictionary<string, string>();

        private static readonly ILogger Logger = Log.ForContext<QueryController>();

        public ICiDashboardService CiDashboardService { get; set; }
        
        public ICiServerService CiServerService { get; set; }

        public async Task SendMessage(string connectionId, string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            await Task.Run(() => hubContext.Clients.Client(connectionId).sendMessage(new { Status = "Info", Message = message }));
        }

        public async Task AddBuilds(string username, string connectionId)
        {
            var userProjects = await this.CiDashboardService.GetProjects(username);
            var buildIds = userProjects
                .SelectMany(p => p.Builds
                    .Where(b => !string.IsNullOrEmpty(b.CiExternalId))
                    .Select(b => b.CiExternalId)
                    .ToList())
                .ToList();

            BuildsPerConnId.AddOrUpdate(connectionId, buildIds, (oldkey, oldvalue) => buildIds);

            Logger.Debug("Start retrieving builds for {user} and {connectionId}", username, connectionId);

            Parallel.ForEach(buildIds,
                build =>
                {
                    if(!BuildsToBeRefreshed.ContainsKey(build))
                        BuildsToBeRefreshed.TryAdd(build, build);
                });


            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            var mappedUserProjects = Mapper.Map<IEnumerable<Project>, IEnumerable<Models.Project>>(userProjects);
            await Task.Run(() => hubContext.Clients.Client(connectionId).sendProjects(mappedUserProjects.ToJson()));
            await Task.Run(() => hubContext.Clients.Client(connectionId).sendMessage(new { Status = "Info", Message = "Your builds are being retrieved" }));
        }

        public async Task RemoveBuilds(string connectionId)
        {
            var builds = new List<string>();
            if (BuildsPerConnId.ContainsKey(connectionId))
                BuildsPerConnId.TryRemove(connectionId, out builds);

            Parallel.ForEach(builds,
                build =>
                {
                    var exists = BuildsPerConnId.Values
                        .SelectMany(b => b)
                        .Contains(build);
                    if(exists)
                        return;
                    string bOut;
                    BuildsToBeRefreshed.TryRemove(build, out bOut);
                });

            Logger.Debug("Remove builds for {connectionId}", connectionId);
        }

        public async Task RefreshBuilds(string connectionId)
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();

                var connectionIdToRefresh = string.IsNullOrEmpty(connectionId)
                    ? BuildsPerConnId.Keys
                    : new List<string> { connectionId };
                Parallel.ForEach(connectionIdToRefresh,
                    connId =>
                    {
                        Logger.Debug("Refreshing build result for {connectionId}", connId);
                        hubContext.Clients.Client(connId)
                            .sendMessage(new { Status = "Info", Message = "Your builds are being refreshed" });
                        hubContext.Clients.Client(connId).startRefresh(new { Status = "start" });
                    });

                var buildsToRefresh = string.IsNullOrEmpty(connectionId)
                    ? BuildsToBeRefreshed.Keys
                    : BuildsPerConnId[connectionId];
                var buildsToRetrieve = buildsToRefresh
                    .Select(buildId => this.GetLastBuildResult(hubContext, buildId))
                    .ToList();

                await Task.WhenAll(buildsToRetrieve);
                hubContext.Clients.All.stopRefresh(new {Status = "stop"});
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error refreshing builds...");
            }
        }

        // only needed because only Hangfire pro supports async calls
        public void RefreshBuildsSync()
        {
            RefreshBuilds(null).Wait();
        }

        public async Task UpdateProject(string connectionId, int oldId, Models.Project project)
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
                hubContext.Clients.Client(connectionId)
                    .sendProjectUpdate(new { OldId = oldId, Project = project });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating project...");
            }     
        }

        public async Task UpdateBuild(string connectionId, int oldId, Build build)
        {
            try
            {
                if (!BuildsToBeRefreshed.ContainsKey(build.CiExternalId))
                    BuildsToBeRefreshed.TryAdd(build.CiExternalId, build.CiExternalId);
                if(BuildsPerConnId.ContainsKey(connectionId))
                {
                    if(!BuildsPerConnId[connectionId].Contains(build.CiExternalId))
                        BuildsPerConnId[connectionId].Add(build.CiExternalId);
                }

                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
                hubContext.Clients.Client(connectionId)
                    .sendBuildUpdate(new { OldId = oldId, Build = build });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating build...");
            }
        }

        public async Task RequestAllProjectBuilds(string connectionId)
        {
            try
            {
                var allProjectBuilds = await this.CiServerService.GetAllProjectBuilds();
                var mappedProjectBuilds = Mapper.Map<IEnumerable<CiBuild>, IEnumerable<Models.ProjectBuild>>(allProjectBuilds);

                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
                hubContext.Clients.Client(connectionId)
                    .sendProjectBuilds(mappedProjectBuilds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error requesting all project builds...");
            }
        }

        private async Task GetLastBuildResult(IHubContext hubContext, string buildId)
        {
            try
            {
                var lastBuildResult = await this.CiServerService.LastBuildResult(buildId);
                var mappedBuildResult = Mapper.Map<CiBuildResult, Models.Build>(lastBuildResult);

                var connIds = BuildsPerConnId.Where(b => b.Value.Contains(mappedBuildResult.CiExternalId))
                    .Select(d => d.Key);

                foreach(var connectionId in connIds)
                {
                    Logger.Debug(
                        "Sending build result for {buildId} to {connectionId}",
                        mappedBuildResult.CiExternalId,
                        connectionId);
                    hubContext.Clients.Client(connectionId)
                        .sendBuildResult(mappedBuildResult);
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Error getting last build result for {buildId}...", buildId);
            }
        }
    }
}