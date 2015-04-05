using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Extensions;
using CIDashboard.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class RefreshInformation : IRefreshInformation
    {
        private static readonly ILogger Logger = Log.ForContext<RefreshInformation>();
   
        internal static ConcurrentDictionary<string, List<string>> BuildsPerConnId = new ConcurrentDictionary<string, List<string>>();
        internal static ConcurrentDictionary<string, string> Builds = new ConcurrentDictionary<string, string>();

        public ICiDashboardService CiDashboardService { get; set; }

        public async Task AddBuilds(string username, string connectionId)
        {
            var userProjects = await this.CiDashboardService.GetProjects(username);
            var builds = userProjects
                .SelectMany(p => p.Builds.Select(b => b.CiExternalId).ToList())
                .ToList();
            if (!BuildsPerConnId.ContainsKey(connectionId))
            {                
                BuildsPerConnId.TryAdd(connectionId, builds);

                Logger.Debug("Start retrieving builds for {user} and {connectionId}", username, connectionId);
            }
            else
            {
                Logger.Debug("Refresh builds for {user} and {connectionId}", username, connectionId);
            }

            Parallel.ForEach(builds, build =>
            {
                if(!Builds.ContainsKey(build))
                    Builds.TryAdd(build, build);
            });


            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            var mappedUserProjects = Mapper.Map<IEnumerable<Project>, IEnumerable<Models.Project>>(userProjects);
            await Task.Run(async () => hubContext.Clients.Client(connectionId).sendProjects(mappedUserProjects.ToJson()));
            await Task.Run(async () =>
                        hubContext.Clients.Client(connectionId)
                            .sendMessage(new {Status = "Info", Message = "Your builds are being retrieved"}));
        }

        public async Task RemoveBuilds(string connectionId)
        {
            var builds = new List<string>();
            if (BuildsPerConnId.ContainsKey(connectionId))
                BuildsPerConnId.TryRemove(connectionId, out builds);

            Parallel.ForEach(builds, build =>
            {
                var exists = BuildsPerConnId.Values.SelectMany(b=>b).Contains(build);
                if (!exists)
                {
                    string b;
                    Builds.TryRemove(build, out b);
                }
            });

            Logger.Debug("Remove builds for {connectionId}", connectionId);
        }

        public async Task RefreshBuilds()
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            foreach (var connectionId in BuildsPerConnId.Keys)
            {
                Logger.Debug("Refreshing builds for {connectionId}", connectionId);
                await Task.Run(async () =>
                    hubContext.Clients.Client(connectionId)
                        .sendMessage(new {Status = "Info", Message = "Your builds are being refreshed"}));
            }
        }
    }
}