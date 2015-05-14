using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Infrastructure.Interfaces
{
    public interface IConnectionsManager
    {
        ConcurrentDictionary<string, List<string>> BuildsPerConnId { get; }
        
        ConcurrentDictionary<string, string> BuildsToBeRefreshed { get; }

        Task AddBuildConfigs(string connectionId, IEnumerable<BuildConfig> buildsConfigs);

        Task UpdateBuildConfigs(string connectionId, IEnumerable<BuildConfig> buildsConfigs);
        
        Task RemoveBuildConfig(string connectionId, BuildConfig buildConfig);

        Task RemoveAllBuildConfigs(string connectionId);
    }
}