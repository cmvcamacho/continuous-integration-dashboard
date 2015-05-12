using AutoMapper;
using CIDashboard.Domain.Entities;
using Microsoft.Ajax.Utilities;

namespace CIDashboard.Web.MappingProfiles
{
    public class ViewModelProfilers : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Data.Entities.Project, Models.Project>()
                .ForMember(dest => dest.Builds, opt => opt.MapFrom(src => src.BuildConfigs));
            Mapper.CreateMap<Models.Project, Data.Entities.Project>()
                .ForMember(dest => dest.BuildConfigs, opt => opt.MapFrom(src => src.Builds));

            Mapper.CreateMap<Data.Entities.BuildConfig, Models.BuildConfig>();
            Mapper.CreateMap<Models.BuildConfig, Data.Entities.BuildConfig>();

            Mapper.CreateMap<CiSource, Models.CiSource>();

            Mapper.CreateMap<CiBuildResult, Models.Build>()
                .ForMember(dest => dest.CiExternalId, opt => opt.MapFrom(src => src.BuildId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BuildName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()));

            Mapper.CreateMap<CiBuildConfig, Models.BuildConfig>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CiExternalId, opt => opt.MapFrom(src => src.Id));
        }
    }
}