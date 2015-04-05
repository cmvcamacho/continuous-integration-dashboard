using AutoMapper;
using CIDashboard.Domain.Entities;
using TeamCitySharp.DomainEntities;

namespace CIDashboard.Domain.CompositionRoot.Profilers
{
    public class TeamCityProfiler : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Project, CiProject>();
        }
    }
}
