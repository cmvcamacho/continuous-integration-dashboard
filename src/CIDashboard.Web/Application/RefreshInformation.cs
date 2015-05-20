using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Application.Interfaces;
using CIDashboard.Web.Models;
using Microsoft.AspNet.SignalR;
using Serilog;

namespace CIDashboard.Web.Application
{
    public class RefreshInformation : IRefreshInformation
    {
        private static readonly ILogger Logger = Log.ForContext<RefreshInformation>();

        public IInformationQuery InfoQuery { get; set; }

        public IConnectionsManager ConnectionsManager { get; set; }

        public async Task SendRefreshBuildResults(string connectionId)
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CiDashboardHub>();
                
                var connectionIdToRefresh = string.IsNullOrEmpty(connectionId)
                    ? this.ConnectionsManager.BuildsPerConnId.Keys
                    : new List<string> { connectionId };

                Parallel.ForEach(connectionIdToRefresh,
                    connId =>
                    {
                        Logger.Debug("Refreshing build result for {connectionId}", connId);
                        hubContext.Clients.Client(connId)
                            .SendMessage(new FeedbackMessage { Status = "Info", Message = "Your builds are being refreshed" });
                        hubContext.Clients.Client(connId).StartRefresh(new RefreshStatus { Status = "start" });
                    });

                var buildsToRefresh = string.IsNullOrEmpty(connectionId)
                    ? this.ConnectionsManager.BuildsToBeRefreshed.Keys
                    : this.ConnectionsManager.BuildsPerConnId[connectionId];
                var buildsToRetrieve = buildsToRefresh
                    .Select(buildId => this.GetLastBuildResult(hubContext, connectionId, buildId))
                    .ToList();

                await Task.WhenAll(buildsToRetrieve);
                hubContext.Clients.All.StopRefresh(new RefreshStatus { Status = "stop" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error refreshing builds...");
            }
        }

        // only needed because only Hangfire pro supports async calls
        public void SendRefreshBuildResultsSync()
        {
            this.SendRefreshBuildResults(null).Wait();
        }

        private async Task GetLastBuildResult(IHubContext hubContext, string connectionId, string buildId)
        {
            try
            {
                var lastBuildResult = await this.InfoQuery.GetLastBuildResult(buildId);

                var connIds = string.IsNullOrEmpty(connectionId)
                    ? this.ConnectionsManager.BuildsPerConnId.Where(b => b.Value.Contains(lastBuildResult.CiExternalId)).Select(d => d.Key)
                    : new List<string> { connectionId };

                foreach (var connId in connIds)
                {
                    Logger.Debug(
                        "Sending build result for {buildId} to {connectionId}",
                        lastBuildResult.CiExternalId,
                        connId);
                    hubContext.Clients.Client(connId)
                        .SendBuildResult(lastBuildResult);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting last build result for {buildId}...", buildId);
            }
        }
    }
}