using System.Configuration;
using Autofac;
using CIDashboard.Domain.Services;
using TeamCitySharp;

namespace CIDashboard.Domain.CompositionRoot
{
    public class CiServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<TeamCityClient>()
                .As<ITeamCityClient>()
                .WithParameter("hostName", ConfigurationManager.AppSettings["TeamcityHostname"])
                .WithParameter("useSsl", bool.Parse(ConfigurationManager.AppSettings["TeamcityUseSsl"]))
                .ExternallyOwned()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
            builder
                .RegisterType<TeamCityService>()
                .As<ICiServerService>()
                .WithParameter("username", ConfigurationManager.AppSettings["TeamcityUsername"])
                .WithParameter("password", ConfigurationManager.AppSettings["TeamcityPassword"])
                .ExternallyOwned()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

        }
    }
}