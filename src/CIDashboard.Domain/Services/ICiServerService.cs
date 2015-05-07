using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Domain.Entities;

namespace CIDashboard.Domain.Services
{
    public interface ICiServerService
    {
        CiSource Source { get; }

        Task<IEnumerable<CiBuild>> GetAllProjectBuilds();

        Task<CiBuildResult> LastBuildResult(string buildId);
    }
}
