using System;
using System.Data.Entity;
using CIDashboard.Data.Entities;

namespace CIDashboard.Data.Interfaces
{
    public interface ICiDashboardContext : IDisposable
    {
        DbSet<Build> Builds { get; set; }
        DbSet<Project> Projects { get; set; }
    }
}