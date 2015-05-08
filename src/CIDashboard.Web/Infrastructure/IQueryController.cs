using System.Threading.Tasks;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Infrastructure
{
    public interface IQueryController
    {
        Task SendMessage(string connectionId, string message);

        Task AddBuilds(string username, string connectionId);

        Task RemoveBuilds(string connectionId);

        Task RefreshBuilds(string connectionId);

        Task RequestAllProjectBuilds(string connectionId);

        void RefreshBuildsSync();

        Task UpdateProject(string connectionId, int oldId, Project project);

        Task UpdateBuild(string connectionId, int oldId, Build build);
    }
}