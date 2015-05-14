using Autofac;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.Infrastructure.Interfaces;
using Hangfire;

namespace CIDashboard.Web.CompositionRoot
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ConnectionsManager>()
                .As<IConnectionsManager>()
                .ExternallyOwned()
                .PropertiesAutowired()
                .SingleInstance();

            builder
                .RegisterType<CommandProcessor>()
                .As<ICommandProcessor>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<RefreshInformation>()
                .As<IRefreshInformation>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<InformationQuery>()
                .As<IInformationQuery>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<BackgroundJobClient>()
                .As<IBackgroundJobClient>()
                .InstancePerLifetimeScope();
        }
    }
}