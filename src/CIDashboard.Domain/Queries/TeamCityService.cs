using System.Collections.Generic;
using AutoMapper;
using CIDashboard.Domain.Entities;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace CIDashboard.Domain.Queries
{
    public class TeamCityService : ICiServerService
    {
        private readonly TeamCityClient _client;

        public TeamCityService(string url, string username, string password)
        {
            _client = new TeamCityClient(url);
            _client.Connect(username, password);
        }

        public List<CiProject> AllProjects()
        {
            //var projects = _client.Projects.All();
            //var mappedProjects = Mapper.Map<IEnumerable<Project>, IEnumerable<CiProject>>(projects);
            throw new System.NotImplementedException();
        }

        public CiBuild LastBuildByBuildConfigId(string buildConfigId)
        {
            throw new System.NotImplementedException();
        }
    }
}
