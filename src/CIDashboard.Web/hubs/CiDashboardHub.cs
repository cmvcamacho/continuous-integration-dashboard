using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CIDashboard.Data.Interfaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using CIDashboard.Data;
using Autofac;

namespace CIDashboard.Web.hubs
{
    [Authorize]
    [HubName("ciHub")]
    public class CiDashboardHub : Hub
    {
        private static readonly object objLock = new object();
        internal static ConcurrentDictionary<string, string> ProjectsPerConnId;

        private readonly ILifetimeScope _hubLifetimeScope;
        private readonly ICiDashboardService _ciDashboardService;

        public CiDashboardHub(ILifetimeScope lifetimeScope)
        {
            lock (objLock)
            {
                ProjectsPerConnId = new ConcurrentDictionary<string, string>();
            }

            // http://autofac.readthedocs.org/en/latest/integration/signalr.html
            // Create a lifetime scope for the hub.
            _hubLifetimeScope = lifetimeScope.BeginLifetimeScope();

         //   _ciDashboardService = _hubLifetimeScope.Resolve<ICiDashboardService>();
        }

        public void Hello()
        {
            Clients.All.hello("1");
        }

        public override Task OnConnected()
        {
            var userName = Context.User.Identity.Name;
            var connId = Context.ConnectionId;

            string projects;
            if (ProjectsPerConnId.ContainsKey(connId))
                ProjectsPerConnId.TryRemove(connId, out projects);

            projects = userName;
            ProjectsPerConnId.TryAdd(connId, projects);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connId = Context.ConnectionId;

            string projects;
            if (ProjectsPerConnId.ContainsKey(connId))
                ProjectsPerConnId.TryRemove(connId, out projects);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userName = Context.User.Identity.Name;
            var connId = Context.ConnectionId;

            string projects;
            if (!ProjectsPerConnId.ContainsKey(connId))
            {
                projects = userName;
                ProjectsPerConnId.TryAdd(connId, projects);
            }

            return base.OnReconnected();
        }
    }
}