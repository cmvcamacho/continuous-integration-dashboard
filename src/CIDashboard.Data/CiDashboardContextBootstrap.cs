using System.Linq;
using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data
{
    public class CiDashboardContextBootstrap : ICiDashboardContextBootstrap
    {
        private readonly ICiDashboardContextFactory factory;

        public CiDashboardContextBootstrap(ICiDashboardContextFactory factory)
        {
            this.factory = factory;
        }

        public void InitiateDatabase()
        {
            using (var ctx = factory.Create())
            {
                ctx.Projects.FirstOrDefault();
            }
        }
    }
}
