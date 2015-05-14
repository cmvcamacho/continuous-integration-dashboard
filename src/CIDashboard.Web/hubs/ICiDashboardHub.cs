using System.Collections.Generic;
using CIDashboard.Web.Models;

namespace CIDashboard.Web.Hubs
{
    public interface ICiDashboardHub
    {
        void SendUpdatedProject(ProjectUpdated project);

        void SendUpdatedBuild(BuildConfigUpdated build);

        void SendMessage(FeedbackMessage message);

        void SendProjectsAndBuildConfigs(IEnumerable<Project> projects);

        void SendProjectBuilds(IEnumerable<BuildConfig> builds);

        void SendBuildResult(Build build);

        void StartRefresh(RefreshStatus status);

        void StopRefresh(RefreshStatus status);
    }
}