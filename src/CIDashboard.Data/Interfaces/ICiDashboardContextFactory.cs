using System.Data.Entity;

namespace CIDashboard.Data.Interfaces
{
    public interface ICiDashboardContextFactory
    {
        ICiDashboardContext Create();
    }
}