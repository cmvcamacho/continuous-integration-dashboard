using System.Collections.Concurrent;
using CIDashboard.Data.Interfaces;
using CIDashboard.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class RefreshInformation : IRefreshInformation
    {
        private static readonly ILogger Logger = Log.ForContext<RefreshInformation>();

        private readonly object _objLock = new object();

        private readonly ICiDashboardService _ciDashboardService;
        
        internal static ConcurrentDictionary<string, string> ProjectsPerConnId { get; set; }

        public RefreshInformation()
        {
            lock (this._objLock)
            {
                ProjectsPerConnId = new ConcurrentDictionary<string, string>();
            }
        }

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
                hubContext.Clients.Client(connectionId).sendMessage("Your builds are being refreshed");
            }
        }
    }
}