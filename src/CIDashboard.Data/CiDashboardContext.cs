using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;

namespace CIDashboard.Data
{
    public class CiDashboardContext : DbContext, ICiDashboardContext
    {
        public CiDashboardContext()
            : base("CiDashboardContext")
        {
            Database.SetInitializer(new MigrateToLatestVersion<CiDashboardContext, MigrationsConfiguration<CiDashboardContext>>());
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

    public class MigrateToLatestVersion<TDataContext, TMigrationsConfiguration> : MigrateDatabaseToLatestVersion<TDataContext, TMigrationsConfiguration>
        where TDataContext : DbContext
        where TMigrationsConfiguration : DbMigrationsConfiguration<TDataContext>, new()
    {
        public MigrateToLatestVersion() : base(true) { }

        public override void InitializeDatabase(TDataContext context)
        {
            try
            {
                base.InitializeDatabase(context);
            }
            catch (Exception)
            {
                new DropCreateDatabaseAlways<TDataContext>().InitializeDatabase(context);
            }
        }
    }

    public class MigrationsConfiguration<TDataContext> : DbMigrationsConfiguration<TDataContext>
        where TDataContext : DbContext
    {
        public MigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
}
