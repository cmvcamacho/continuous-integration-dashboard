using System;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Interfaces;
using CIDashboard.Web.Infrastructure.Interfaces;
using CIDashboard.Web.Models;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class CommandProcessor : ICommandProcessor 
    {
        private static readonly ILogger Logger = Log.ForContext<CommandProcessor>();
      
        public ICiDashboardService CiDashboardService { get; set; }

        public async Task<Project> AddNewProject(string username, Project project)
        {
            try
            {
                Logger.Debug("Creating a new project {project}", project);

                var mappedInProj = Mapper.Map<Project, Data.Entities.Project>(project);
                var createdProject = await CiDashboardService.AddProject(username, mappedInProj);

                var mappedOutProj = Mapper.Map<Data.Entities.Project, Project>(createdProject);
                Logger.Debug("Project created {project}", mappedOutProj);
                return mappedOutProj;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding new project...");
            }

            return null;
        }

        public async Task<bool> UpdateProjectName(int projectId, string projectName)
        {
            try
            {
                Logger.Debug("Updating project name for {projectId}", projectId);

                await CiDashboardService.UpdateProjectName(projectId, projectName);

                Logger.Debug("Updated project name to {projectName} for {projectId}", projectName, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating project name...");
            }

            return false;
        }

        public async Task<bool> UpdateProjectOrder(int projectId, int position)
        {
            try
            {
                Logger.Debug("Updating project order for {projectId}", projectId);

                await CiDashboardService.UpdateProjectOrder(projectId, position);

                Logger.Debug("Updated project order to {position} for {projectId}", position, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating project order...");
            }

            return false;
        }

        public async Task<Project> RemoveProject(int projectId)
        {
            try
            {
                Logger.Debug("Removing project with ID {projectId}", projectId);

                var project = await CiDashboardService.RemoveProject(projectId);
                if(project == null)
                    return null;

                var mappedOutProj = Mapper.Map<Data.Entities.Project, Project>(project);

                Logger.Debug("Removed project with ID {projectId}", projectId);
                return mappedOutProj;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing project...");
            }

            return null;
        }

        public async Task<BuildConfig> AddBuildConfigToProject(int projectId, BuildConfig build)
        {
            try
            {
                Logger.Debug("Adding build {build} to project {projectId}", build, projectId);

                var mappedInBuild = Mapper.Map<BuildConfig, Data.Entities.BuildConfig>(build);
                var createdBuild = await CiDashboardService.AddBuildConfigToProject(projectId, mappedInBuild);

                var mappedOutBuild = Mapper.Map<Data.Entities.BuildConfig, BuildConfig>(createdBuild);
                Logger.Debug("Added build {build} to project {projectId}", build, projectId);
                return mappedOutBuild;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding build to project...");
            }

            return null;
        }

        public async Task<BuildConfig> RemoveBuildConfig(int buildId)
        {
            try
            {
                Logger.Debug("Removing build with ID {buildId}", buildId);

                var build = await CiDashboardService.RemoveBuildConfig(buildId);
                if (build == null)
                    return null;

                var mappedOutBuild = Mapper.Map<Data.Entities.BuildConfig, BuildConfig>(build);

                Logger.Debug("Removed build with ID {buildId}", buildId);
                return mappedOutBuild;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing build...");
            }

            return null;
        }

        public async Task<bool> UpdateBuildConfigExternalId(int buildId, string buildName, string externalId)
        {
            try
            {
                Logger.Debug("Updating build name and external id for {buildId}", buildId);

                await CiDashboardService.UpdateBuildConfigExternalId(buildId, buildName, externalId);

                Logger.Debug("Updated build name to {buildName} and external id {externalId} for {buildId}", buildName, externalId, buildId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating build name and external id...");
            }

            return false;
        }

        public async Task<bool> UpdateBuildConfigOrder(int buildId, int position)
        {
            try
            {
                Logger.Debug("Updating build order for {buildId}", buildId);

                await CiDashboardService.UpdateBuildConfigOrder(buildId, position);

                Logger.Debug("Updated build order to {position} for {buildId}", position, buildId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating build order...");
            }

            return false;
        }
    }
}