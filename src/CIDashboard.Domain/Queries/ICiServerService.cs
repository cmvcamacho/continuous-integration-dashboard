using System.Collections.Generic;
using CIDashboard.Domain.Entities;

namespace CIDashboard.Domain.Queries
{
    public interface ICiServerService
    {
        List<CiProject> AllProjects();

        CiBuild LastBuildByBuildConfigId(string buildConfigId);
    }
}
