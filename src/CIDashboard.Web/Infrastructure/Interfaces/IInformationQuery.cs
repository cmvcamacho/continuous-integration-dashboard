using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Infrastructure.Interfaces
{
    public interface IInformationQuery
    {
        Task<IEnumerable<Project>> GetUserProjectsAndBuildConfigs(string username);

        Task<IEnumerable<BuildConfig>> GetAllProjectBuildConfigs();

        Task<Build> GetLastBuildResult(string buildId);
    }
}