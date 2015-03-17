using System.Threading.Tasks;
using CIDashboard.Web.Infrastructure;
using Hangfire;
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
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRefreshInformation _refreshInformation;

        public CiDashboardHub(IBackgroundJobClient backgroundJobClient, IRefreshInformation refreshInformation)
        {
            // http://autofac.readthedocs.org/en/latest/integration/signalr.html
            // Create a lifetime scope for the hub.
            //_hubLifetimeScope = lifetimeScope.BeginLifetimeScope();

            _backgroundJobClient = backgroundJobClient;
            _refreshInformation = refreshInformation;
        }

        public override Task OnConnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnConnected for {username} and {connectionId}", username, connectionId);

            _backgroundJobClient.Enqueue(() => _refreshInformation.AddBuilds(username, connectionId));

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            Logger.Debug("OnDisconnected for {connectionId}", connectionId);

            _backgroundJobClient.Enqueue(() => _refreshInformation.RemoveBuilds(connectionId));

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            Logger.Debug("OnReconnected for {username} and {connectionId}", username, connectionId);

            _backgroundJobClient.Enqueue(() => _refreshInformation.AddBuilds(username, connectionId));

            return base.OnReconnected();
        }

        //public void Hello()
        //{
        //    Clients.All.hello("1");
        //}
    }
}