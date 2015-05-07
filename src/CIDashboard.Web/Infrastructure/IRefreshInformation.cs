using System.Threading.Tasks;

namespace CIDashboard.Web.Infrastructure
{
    public interface IRefreshInformation 
    {
        Task AddBuilds(string username, string connectionId);

        Task RemoveBuilds(string connectionId);

        Task RefreshBuilds(string connectionId);

        Task RequestAllProjectBuilds(string connectionId);

        void RefreshBuildsSync();
    }
}