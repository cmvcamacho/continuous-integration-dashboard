using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Domain.Entities;

namespace CIDashboard.Domain.Services
{
    public interface ICiServerService
    {
        CiSource Source { get; }

        Task<IEnumerable<CiBuildConfig>> GetAllBuildConfigs();

        Task<CiBuildResult> LastBuildResult(string buildId);
    }
}
