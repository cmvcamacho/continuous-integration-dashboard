using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Infrastructure
{
    public interface ICommandController 
    {
        Task<Project> AddNewProject(string username, Project project);

        Task<bool> UpdateProjectName(int projectId, string projectName);

        Task<bool> UpdateProjectOrder(int projectId, int position);

        Task<bool> RemoveProject(int projectId);

        Task<BuildConfig> AddBuildToProject(int projectId, BuildConfig build);
    }
}