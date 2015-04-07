using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Domain.Entities;
using Serilog;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace CIDashboard.Domain.Services
{
    public class TeamCityService : ICiServerService
    {
        private readonly ITeamCityClient _client;
        private static readonly ILogger Logger = Log.ForContext<TeamCityService>();


        public TeamCityService(ITeamCityClient client)
        {
            Logger.Debug("Connecting to TeamCity as guest");
            this._client = client;
            this._client.ConnectAsGuest();
        }
        
        public TeamCityService(ITeamCityClient client, string username, string password)
        {
            Logger.Debug("Connecting to TeamCity with username {username}", username);
            this._client = client; 
            this._client.Connect(username, password);
        }

        public CiSource Source
        {
            get
            {
                return CiSource.TeamCity;
            }
        }

        public async Task<IEnumerable<CiProject>> GetAllProjectBuilds()
        {
            Logger.Debug("Retrieving from TeamCity all Projects");
            var projects = await Task.Run(() => this._client.Projects.All());
            var mappedProjects = Mapper.Map<IEnumerable<Project>, IEnumerable<CiProject>>(projects);

            return mappedProjects;
        }

        public async Task<CiBuildResult> LastBuildResult(string buildId)
        {
            //return new CiBuildResult
            //{
            //    BuildId = buildId,
            //    Status = CiBuildResultStatus.Success,
            //    Version = "1.1.2.1",
            //    BuildName = "awasas"
            //};

            Logger.Debug("Retrieving from TeamCity last build for {buildId}", buildId);
            var build = await Task.Run(
                () =>
                {
                    try
                    {
                        return _client.Builds.LastBuildByBuildConfigId(buildId);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex, "Error retrieving from TeamCity last build for {buildId}", buildId);
                    }

                    return null;
                });
            if(build == null)
            {
                return null;
            }

            var mappedBuild = Mapper.Map<Build, CiBuildResult>(build);

            Logger.Debug("Retrieving from TeamCity if {buildId} is running", buildId);
            var isBuildRunning = await Task.Run(() =>
                                {
                    try
                    {
                        return _client
                            .Builds
                            .ByBuildLocator(BuildLocator.WithDimensions(buildType: BuildTypeLocator.WithId(mappedBuild.BuildId), running: true))
                                .Count > 0;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex, "Error retrieving from TeamCity if {buildId} is running", buildId);
                    }

                    return false;
                });

            if (isBuildRunning)
                mappedBuild.Status = CiBuildResultStatus.Running;

            return mappedBuild;
        }
    }
}
