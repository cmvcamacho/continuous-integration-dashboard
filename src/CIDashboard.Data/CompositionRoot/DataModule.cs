using Autofac;
using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data.CompositionRoot
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CiDashboardContextFactory>()
                .As<ICiDashboardContextFactory>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CiDashboardContext>()
                .As<ICiDashboardContext>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CiDashboardService>()
                .As<ICiDashboardService>()
                .ExternallyOwned()
                .InstancePerLifetimeScope();
        }
    }
}