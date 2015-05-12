using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Infrastructure
{
    public interface IQueryController
    {
        Task SendMessage(string connectionId, string message);

        Task SendUserProjectsAndBuildConfigs(string username, string connectionId);

        Task RemoveUserBuildsConfigs(string connectionId);

        Task SendRefreshBuildResults(string connectionId);

        void SendRefreshBuildResultsSync();

        Task RequestAllProjectBuilds(string connectionId);

        Task SendUpdatedProject(string connectionId, int oldId, Project project);

        Task SendUpdatedBuild(string connectionId, int oldId, BuildConfig build);
    }
}