using System.Collections.Concurrent;
using System.Collections.Generic;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class RefreshInformation : IRefreshInformation
    {
        private static readonly ILogger Logger = Log.ForContext<RefreshInformation>();
   
        internal static ConcurrentDictionary<string, string> ProjectsPerConnId = new ConcurrentDictionary<string, string>();

        public ICiDashboardService CiDashboardService { get; set; }

        public void AddBuilds(string username, string connectionId)
        {
            if(!ProjectsPerConnId.ContainsKey(connectionId))
            {
                var projects = username;
                ProjectsPerConnId.TryAdd(connectionId, projects);

                Logger.Debug("Start retrieving builds for {user} and {connectionId}", username, connectionId);
            }
            else
            {
                Logger.Debug("Refresh builds for {user} and {connectionId}", username, connectionId);
            }

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            hubContext.Clients.Client(connectionId).sendMessage("Your builds are being retrieved");

            //this.CiDashboardService.GetProjects(username);

            hubContext.Clients.Client(connectionId).sendProjects(new []
            {
                new Project{Description = "teste", Builds = new []{new Build{Description = "asas"}}}
            });
        }

        public void RemoveBuilds(string connectionId)
        {
            string projects;
            if (ProjectsPerConnId.ContainsKey(connectionId))
                ProjectsPerConnId.TryRemove(connectionId, out projects);

            Logger.Debug("Remove builds for {connectionId}", connectionId);
        }

        public void RefreshBuilds()
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
            foreach (var connectionId in ProjectsPerConnId.Keys)
            {
                Logger.Debug("Refreshing builds for {connectionId}", connectionId);
                hubContext.Clients.Client(connectionId).sendMessage("Your builds are being refreshed");
            }
        }
    }
}