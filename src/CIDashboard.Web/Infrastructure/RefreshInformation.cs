using System.Collections.Concurrent;
using System.Collections.Generic;
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
   
        internal static ConcurrentDictionary<string, string> ProjectsPerConnId = new ConcurrentDictionary<string, string>();

        public ICiDashboardService CiDashboardService { get; set; }

        public async void AddBuilds(string username, string connectionId)
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
            hubContext.Clients.Client(connectionId).sendMessage(new {Status = "Info", Message = "Your builds are being retrieved" });

            var userProjects = await this.CiDashboardService.GetProjects(username);

            hubContext.Clients.Client(connectionId).sendProjects(Mapper.Map<IEnumerable<Data.Entities.Project>, IEnumerable<Models.Project>>(userProjects).ToJson());
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
                hubContext.Clients.Client(connectionId).sendMessage(new { Status = "Info", Message = "Your builds are being refreshed" });
            }
        }
    }
}