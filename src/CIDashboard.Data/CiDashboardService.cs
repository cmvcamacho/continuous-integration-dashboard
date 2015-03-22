using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data
{
    public class CiDashboardService : ICiDashboardService
    {
        public ICiDashboardContextFactory CtxFactory { get; set; }

        public async Task<IEnumerable<Project>> GetProjects(string username)
        {
            using (var context = CtxFactory.Create())
            {
                return await context.Projects
                    .Where(p => p.User == username)
                    .Include(p => p.Builds)
                    .OrderBy(p => p.Order)
                    .ToListAsync();
            }
        }
    }
}
