using System.Threading.Tasks;
using CIDashboard.Web.Infrastructure;
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
        private readonly IRefreshInformation _refreshInformation;

        public CiDashboardHub(IRefreshInformation refreshInformation)
        {
            // http://autofac.readthedocs.org/en/latest/integration/signalr.html
            // Create a lifetime scope for the hub.
            //_hubLifetimeScope = lifetimeScope.BeginLifetimeScope();

            _refreshInformation = refreshInformation;
        }

        public override async Task OnConnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnConnected for {username} and {connectionId}", username, connectionId);

            await _refreshInformation.AddBuilds(username, connectionId);

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            Logger.Debug("OnDisconnected for {connectionId}", connectionId);

            await _refreshInformation.RemoveBuilds(connectionId);

            await base.OnDisconnected(stopCalled);
        }

        public override async Task OnReconnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnReconnected for {username} and {connectionId}", username, connectionId);

            await _refreshInformation.AddBuilds(username, connectionId);

            await base.OnReconnected();
        }

        public async Task RequestRefresh()
        {
            var connectionId = Context.ConnectionId;
            await _refreshInformation.RefreshBuilds(connectionId);
        }
    }
}