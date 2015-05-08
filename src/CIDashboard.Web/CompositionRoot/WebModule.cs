using Autofac;
using CIDashboard.Web.Infrastructure;
using Hangfire;

namespace CIDashboard.Web.CompositionRoot
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<QueryController>()
                .As<IQueryController>()
                .ExternallyOwned()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CommandController>()
                .As<ICommandController>()
                .ExternallyOwned()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<BackgroundJobClient>()
                .As<IBackgroundJobClient>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();
        }
    }
}