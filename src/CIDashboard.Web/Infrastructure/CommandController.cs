using System;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Interfaces;
using CIDashboard.Web.Models;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class CommandController : ICommandController 
    {
        private static readonly ILogger Logger = Log.ForContext<CommandController>();
      
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
            throw new System.NotImplementedException();
        }

        public async Task<bool> RemoveProject(int projectId)
        {
            try
            {
                Logger.Debug("Removing project with ID {projectId}", projectId);

                await CiDashboardService.RemoveProject(projectId);

                Logger.Debug("Removed project with ID {projectId}", projectId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing project...");
            }

            return false;
        }

        public async Task<Build> AddBuildToProject(int projectId, Build build)
        {
            try
            {
                Logger.Debug("Adding build {build} to project {projectId}", build, projectId);

                var mappedInBuild = Mapper.Map<Build, Data.Entities.Build>(build);
                var createdBuild = await CiDashboardService.AddBuildToProject(projectId, mappedInBuild);

                var mappedOutBuild = Mapper.Map<Data.Entities.Build, Build>(createdBuild);
                Logger.Debug("Added build {build} to project {projectId}", build, projectId);
                return mappedOutBuild;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding build to project...");
            }

            return null;
        }
    }
}