using Autofac;
using CIDashboard.Domain.Queries;

namespace CIDashboard.Domain.CompositionRoot
{
    public class CiServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<TeamCityService>()
                .As<ICiServerService>()
                .ExternallyOwned()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
        }
    }
}