using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data
{
    public class CiDashboardContextFactory: ICiDashboardContextFactory
    {
        public ICiDashboardContext Create()
        {
            return new CiDashboardContext();
        }
    }
}
