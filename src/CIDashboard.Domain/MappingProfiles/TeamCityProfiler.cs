using System;
using AutoMapper;
using CIDashboard.Domain.Entities;
using TeamCitySharp.DomainEntities;

namespace CIDashboard.Domain.MappingProfiles
{
    public class TeamCityProfiler : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Project, CiProject>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.WebUrl))
                .ForMember(dest => dest.Builds, opt => opt.MapFrom(src => src.BuildTypes.BuildType))
                .AfterMap((src, dest) => dest.CiSource = CiSource.TeamCity);

            Mapper.CreateMap<BuildConfig, CiBuild>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.WebUrl))
                .AfterMap((src, dest) => dest.CiSource = CiSource.TeamCity);

            Mapper.CreateMap<Build, CiBuildResult>()
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.BuildName, opt => opt.MapFrom(src => src.BuildConfig.Name))
                .ForMember(dest => dest.BuildId, opt => opt.MapFrom(src => src.BuildConfig.Id))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.WebUrl))
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(
                        src => src.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase)
                            ? CiBuildResultStatus.Success
                            : CiBuildResultStatus.Failure))
                .AfterMap((src, dest) => dest.CiSource = CiSource.TeamCity);
        }
    }
}
