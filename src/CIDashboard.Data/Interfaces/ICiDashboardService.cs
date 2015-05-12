using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Data.Entities;

namespace CIDashboard.Data.Interfaces
{
    public interface ICiDashboardService
    {
        Task<IEnumerable<Project>> GetProjects(string username);

        Task<Project> AddProject(string username, Project project);

        Task<bool> UpdateProjectName(int projectId, string projectName);

        Task<bool> UpdateProjectOrder(int projectId, int position);

        Task<bool> RemoveProject(int projectId);

        Task<BuildConfig> AddBuildConfigToProject(int projectId, BuildConfig buildConfig);
    }
}