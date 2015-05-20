using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Services;
using CIDashboard.Web.Application.Interfaces;
using CIDashboard.Web.Models;
using Serilog;

namespace CIDashboard.Web.Application
{
    public class ConnectionsManager : IConnectionsManager
    {
        private static readonly ConcurrentDictionary<string, List<string>> buildsPerConnId = new ConcurrentDictionary<string, List<string>>();

        private static readonly ConcurrentDictionary<string, string> buildsToBeRefreshed = new ConcurrentDictionary<string, string>();

        private static readonly ILogger Logger = Log.ForContext<ConnectionsManager>();

        public ICiDashboardService CiDashboardService { get; set; }
        
        public ICiServerService CiServerService { get; set; }

        public ConcurrentDictionary<string, List<string>> BuildsPerConnId
        {
            get
            {
                return buildsPerConnId;
            }
        }

        public ConcurrentDictionary<string, string> BuildsToBeRefreshed
        {
            get
            {
                return buildsToBeRefreshed;
            }
        }

        public async Task AddBuildConfigs(string connectionId, IEnumerable<BuildConfig> buildsConfigs)
        {
            // remove any existing builds for this connection
            await RemoveAllBuildConfigs(connectionId);

            var buildCiIds = buildsConfigs
                    .Where(b => !string.IsNullOrEmpty(b.CiExternalId) && !b.CiExternalId.StartsWith("-"))
                    .Select(b => b.CiExternalId)
                    .ToList()
                .ToList();

            BuildsPerConnId.AddOrUpdate(connectionId, buildCiIds, (oldkey, oldvalue) => buildCiIds);

            Parallel.ForEach(buildCiIds,
                build =>
                {
                    if (!BuildsToBeRefreshed.ContainsKey(build))
                        BuildsToBeRefreshed.TryAdd(build, build);
                });
        }

        public async Task UpdateBuildConfigs(string connectionId, IEnumerable<BuildConfig> buildsConfigs)
        {
            var buildCiIds = buildsConfigs
                    .Where(b => !string.IsNullOrEmpty(b.CiExternalId) && !b.CiExternalId.StartsWith("-"))
                    .Select(b => b.CiExternalId)
                    .ToList()
                .ToList();

            Parallel.ForEach(buildCiIds,
                build =>
                {
                    if (BuildsPerConnId.ContainsKey(connectionId) && !BuildsPerConnId[connectionId].Contains(build))
                        BuildsPerConnId[connectionId].Add(build);
                });

            Parallel.ForEach(buildCiIds,
                build =>
                {
                    if (!BuildsToBeRefreshed.ContainsKey(build))
                        BuildsToBeRefreshed.TryAdd(build, build);
                });
        }

        public async Task RemoveBuildConfig(string connectionId, BuildConfig buildConfig)
        {
            var buildId = buildConfig.CiExternalId;
            if(BuildsPerConnId.ContainsKey(connectionId) && BuildsPerConnId[connectionId].Contains(buildId))
                BuildsPerConnId[connectionId].Remove(buildId);

            var exists = BuildsPerConnId.Values
                .SelectMany(b => b)
                .Contains(buildId);
            if(exists)
                return;

            string bOut;
            BuildsToBeRefreshed.TryRemove(buildId, out bOut);

            Logger.Debug("Remove build {buildId} for {connectionId}", buildId, connectionId);
        }

        public async Task RemoveAllBuildConfigs(string connectionId)
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
                    if (exists)
                        return;
                    string bOut;
                    BuildsToBeRefreshed.TryRemove(build, out bOut);
                });

            Logger.Debug("Remove builds for {connectionId}", connectionId);
        }
    }
}