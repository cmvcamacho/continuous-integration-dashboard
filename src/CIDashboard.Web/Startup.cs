using System;
using Autofac;
using Autofac.Integration.SignalR;
using CIDashboard.Data.CompositionRoot;
using CIDashboard.Web.CompositionRoot;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Infrastructure;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Serilog;

[assembly: OwinStartup(typeof(CIDashboard.Web.Startup))]

namespace CIDashboard.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureLog();

            var builder = new ContainerBuilder();

            // STANDARD SIGNALR SETUP:
            // Get your HubConfiguration. In OWIN, you'll create one
            // rather than using GlobalHost.
            var config = new HubConfiguration();

            // Register your SignalR hubs.
            // builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterType<CiDashboardHub>().ExternallyOwned();

            // add application modules
            builder.RegisterModule<DataModule>();
            builder.RegisterModule<WebModule>();
            builder.RegisterModule<CiServicesModule>();
            
            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            config.Resolver = new AutofacDependencyResolver(container);

            // OWIN SIGNALR SETUP:
            // Register the Autofac middleware FIRST, then the standard SignalR middleware.
            app.UseAutofacMiddleware(container);

            GlobalHost.DependencyResolver = config.Resolver;

            app.UseHangfire(configHangfire =>
            {
                configHangfire.UseAutofacActivator(container);
                configHangfire.UseSqlServerStorage(
                    "CiDashboardContext",
                    new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(5) });
                configHangfire.UseServer();
            });
            ConfigureHangfireJobs(container);

            app.MapSignalR("/signalr", config);
        }

        private void ConfigureHangfireJobs(IContainer container)
        {
            RecurringJob.AddOrUpdate("RefreshBuilds", () => container.Resolve<IRefreshInformation>().RefreshBuilds(), Cron.Minutely);
        }

        private void ConfigureLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadAppSettings()
                .CreateLogger();
        }
    }
}
