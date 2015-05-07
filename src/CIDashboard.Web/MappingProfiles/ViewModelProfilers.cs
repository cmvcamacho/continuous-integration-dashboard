using AutoMapper;
using CIDashboard.Domain.Entities;

namespace CIDashboard.Web.MappingProfiles
{
    public class ViewModelProfilers : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Data.Entities.Project, Models.Project>();

            Mapper.CreateMap<Data.Entities.Build, Models.Build>();

            Mapper.CreateMap<CiSource, Models.CiSource>();

            Mapper.CreateMap<CiBuildResult, Models.Build>()
                .ForMember(dest => dest.CiExternalId, opt => opt.MapFrom(src => src.BuildId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BuildName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()));

            Mapper.CreateMap<CiBuild, Models.ProjectBuild>();
        }
    }
}