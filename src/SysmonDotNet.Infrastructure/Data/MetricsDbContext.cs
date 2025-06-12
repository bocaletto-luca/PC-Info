using Microsoft.EntityFrameworkCore;
using SysmonDotNet.Core.Models;

namespace SysmonDotNet.Infrastructure.Data;

public class MetricsDbContext : DbContext
{
    public DbSet<MetricRecord> Metrics { get; set; } = null!;

    public MetricsDbContext(DbContextOptions<MetricsDbContext> opts)
        : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetricRecord>()
            .HasKey(m => m.Id);
        base.OnModelCreating(modelBuilder);
    }
}
