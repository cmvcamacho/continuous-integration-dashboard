using System.Collections.Generic;
using System.Threading.Tasks;
using CIDashboard.Data.Entities;

namespace CIDashboard.Data.Interfaces
{
    public interface ICiDashboardService
    {
        Task<IEnumerable<Project>> GetProjects(string username);
    }
}