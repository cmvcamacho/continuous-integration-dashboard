using AutoMapper;

namespace CIDashboard.Web.CompositionRoot.Profilers
{
    public class DbToModelProfilers : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Data.Entities.Project, Models.Project>();
            Mapper.CreateMap<Data.Entities.Build, Models.Build>();
        }
    }
}