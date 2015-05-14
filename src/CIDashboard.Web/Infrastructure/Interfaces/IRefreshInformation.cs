using System.Threading.Tasks;

namespace CIDashboard.Web.Infrastructure.Interfaces
{
    public interface IRefreshInformation
    {
        Task SendRefreshBuildResults(string connectionId);

        void SendRefreshBuildResultsSync();
    }
}