using System.Threading.Tasks;

namespace CIDashboard.Web.Application.Interfaces
{
    public interface IRefreshInformation
    {
        Task SendRefreshBuildResults(string connectionId);

        void SendRefreshBuildResultsSync();
    }
}