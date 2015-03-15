using System.Data.Entity;
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

        }
        public DbSet<Build> Builds { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        }
    }
}
