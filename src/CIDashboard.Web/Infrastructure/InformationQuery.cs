using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Entities;
using CIDashboard.Domain.Services;
using CIDashboard.Web.Infrastructure.Interfaces;
using CIDashboard.Web.Models;
using Serilog;

namespace CIDashboard.Web.Infrastructure
{
    public class InformationQuery : IInformationQuery
    {
        private static readonly ILogger Logger = Log.ForContext<InformationQuery>();

        public ICiDashboardService CiDashboardService { get; set; }
    
        public ICiServerService CiServerService { get; set; }

        public async Task<IEnumerable<Project>> GetUserProjectsAndBuildConfigs(string username)
        {
            try
            {
                var userProjects = await this.CiDashboardService.GetProjects(username);
                var mappedUserProjects = Mapper.Map<IEnumerable<Data.Entities.Project>, IEnumerable<Project>>(userProjects);

                return mappedUserProjects;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error requesting all project for {username}", username);
            }

            return new List<Project>();
        }

        public async Task<IEnumerable<BuildConfig>> GetAllProjectBuildConfigs()
        {
            try
            {
                var allProjectBuilds = await this.CiServerService.GetAllBuildConfigs();
                var mappedBuilds = Mapper.Map<IEnumerable<CiBuildConfig>, IEnumerable<BuildConfig>>(allProjectBuilds);

                return mappedBuilds;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error requesting all project builds...");
            }

            return new List<BuildConfig>();
        }

        public async Task<Build> GetLastBuildResult(string buildId)
        {
            try
            {
                var lastBuildResult = await this.CiServerService.LastBuildResult(buildId);
                var mappedBuild = Mapper.Map<CiBuildResult, Build>(lastBuildResult);

                return mappedBuild;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting last build result for {buildId}...", buildId);
            }

            return null;
        }
    }
}