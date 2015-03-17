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
                .RegisterType<RefreshInformation>()
                .As<IRefreshInformation>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<BackgroundJobClient>()
                .As<IBackgroundJobClient>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();
        }
    }
}