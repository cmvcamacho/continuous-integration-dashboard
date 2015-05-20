using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Application.Interfaces
{
    public interface ICommandProcessor 
    {
        Task<Project> AddNewProject(string username, Project project);

        Task<bool> UpdateProjectName(int projectId, string projectName);

        Task<bool> UpdateProjectOrder(int projectId, int position);

        Task<Project> RemoveProject(int projectId);

        Task<BuildConfig> AddBuildConfigToProject(int projectId, BuildConfig build);

        Task<BuildConfig> RemoveBuildConfig(int buildId);

        Task<bool> UpdateBuildConfigExternalId(int buildId, string buildName, string externalId);

        Task<bool> UpdateBuildConfigOrder(int buildId, int position);

    }
}