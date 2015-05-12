using System;
using System.Data.Entity;
using CIDashboard.Data.Entities;

namespace CIDashboard.Data.Interfaces
{
    public interface ICiDashboardContext : IDisposable
    {
        DbSet<BuildConfig> BuildConfigs { get; set; }

        DbSet<Project> Projects { get; set; }

        int SaveChanges();
    }
}