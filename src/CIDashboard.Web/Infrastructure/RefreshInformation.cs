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

namespace CIDashboard.Web.Infrastructure
{
    public class RefreshInformation : IRefreshInformation
    {
        internal static ConcurrentDictionary<string, List<string>> BuildsPerConnId = new ConcurrentDictionary<string, List<string>>();
        internal static ConcurrentDictionary<string, string> BuildsToBeRefreshed = new ConcurrentDictionary<string, string>();

        private static readonly ILogger Logger = Log.ForContext<RefreshInformation>();

        public ICiDashboardService CiDashboardService { get; set; }
        
        public ICiServerService CiServerService { get; set; }

        public async Task AddBuilds(string username, string connectionId)
        {
            var userProjects = await this.CiDashboardService.GetProjects(username);
            var buildIds = userProjects
                .SelectMany(p => p.Builds.Select(b => b.CiExternalId).ToList())
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
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();

            var connectionIdToRefresh = string.IsNullOrEmpty(connectionId)
                ? BuildsPerConnId.Keys
                : new List<string> { connectionId };
            Parallel.ForEach(connectionIdToRefresh,
                connId =>
                {
                    Logger.Debug("Refreshing build result for {connectionId}", connId);
                    hubContext.Clients.Client(connId).sendMessage(new { Status = "Info", Message = "Your builds are being refreshed" });
                    hubContext.Clients.Client(connId).startRefresh(new { Status = "start" });
                });

            var buildsToRefresh = string.IsNullOrEmpty(connectionId)
                ? BuildsToBeRefreshed.Keys
                : BuildsPerConnId[connectionId];
            var buildsToRetrieve = buildsToRefresh
                .Select(buildId => this.GetLastBuildResult(hubContext, buildId))
                .ToList();

            await Task.WhenAll(buildsToRetrieve);
            hubContext.Clients.All.stopRefresh(new { Status = "stop" });
        }

        // only needed because only Hangfire pro supports async calls
        public void RefreshBuildsSync()
        {
            RefreshBuilds(null).Wait();
        }

        private async Task GetLastBuildResult(IHubContext hubContext, string buildId)
        {
            var lastBuildResult = await this.CiServerService.LastBuildResult(buildId);
            var mappedBuildResult = Mapper.Map<CiBuildResult, Models.Build>(lastBuildResult);

            var connIds = BuildsPerConnId.Where(b => b.Value.Contains(mappedBuildResult.BuildId)).Select(d => d.Key);

            foreach (var connectionId in connIds)
            {
                Logger.Debug(
                    "Sending build result for {buildId} to {connectionId}",
                    mappedBuildResult.BuildId,
                    connectionId);
                hubContext.Clients.Client(connectionId).sendBuildResult(mappedBuildResult);
            }
        }

    }
}