using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Domain.Entities;

namespace CIDashboard.Domain.Services
{
    public interface ICiServerService
    {
        CiSource Source { get; }

        Task<IEnumerable<CiProject>> GetAllProjectBuilds();

        Task<CiBuildResult> LastBuildResult(string buildId);
    }
}
