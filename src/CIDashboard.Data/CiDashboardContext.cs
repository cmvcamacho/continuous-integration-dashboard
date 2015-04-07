using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data
{
    public class CiDashboardContext : DbContext, ICiDashboardContext
    {
        public CiDashboardContext()
            : base("CiDashboardContext")
        {
            Database.SetInitializer(new MigrateToLatestVersion());
        }

        public DbSet<Build> Builds { get; set; }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = true;
            this.Configuration.ValidateOnSaveEnabled = true;
            
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }

    public class MigrateToLatestVersion : MigrateDatabaseToLatestVersion<CiDashboardContext, MigrationsConfiguration>
    {
        public MigrateToLatestVersion() : base(true) { }

        public override void InitializeDatabase(CiDashboardContext context)
        {
            try
            {
                base.InitializeDatabase(context);
            }
            catch (Exception)
            {
                new DropCreateDatabaseAlways<CiDashboardContext>().InitializeDatabase(context);
            }
        }
    }

    public class MigrationsConfiguration : DbMigrationsConfiguration<CiDashboardContext>
    {
        public MigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(CiDashboardContext context)
        {
#if DEBUG
            if(!context.Projects.Any())
            {
                var username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                var projects = new List<Project>
                {
                    new Project
                    {
                        Id = 1,
                        User = username,
                        Name = "MNP Framework"
                    },
                    new Project
                    {
                        Id = 2,
                        User = username,
                        Name = "OIS"
                    }
                };
                projects.First().Builds = new List<Build>
                {
                    new Build
                    {
                        Id = 1,
                        CiExternalId = "bt24",
                        Name = "Mnp Jobs",
                        Project = projects.First()
                    },
                    new Build
                    {
                        Id = 2,
                        CiExternalId = "bt6",
                        Name = "Mnp Services",
                        Project = projects.First()
                    }
                };
                projects.Last().Builds = new List<Build>
                {
                    new Build
                    {
                        Id = 3,
                        CiExternalId = "OIS_Trunk_OisService",
                        Name = "OIS Service",
                        Project = projects.Last()
                    },
                    new Build
                    {
                        Id = 4,
                        CiExternalId = "OIS_Trunk_AcceptanceTests_OisAcceptanceTestsDev6Nightly",
                        Name = "OIS Acceptance Tests (dev6 Nightly)",
                        Project = projects.Last()
                    }
                };
                context.Projects.AddRange(projects);
            
                context.SaveChanges();
            }
#endif
            base.Seed(context);
        }
    }
}
