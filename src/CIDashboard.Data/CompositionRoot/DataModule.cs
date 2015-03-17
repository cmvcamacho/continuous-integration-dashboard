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
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CiDashboardService>()
                .As<ICiDashboardService>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
        }
    }
}