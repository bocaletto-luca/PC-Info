using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SysmonDotNet.Core.Models;
using SysmonDotNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SysmonDotNet.Core.Services;

public class MetricsCollectorService : BackgroundService
{
    private readonly ILogger<MetricsCollectorService> _log;
    private readonly IServiceProvider _provider;
    private readonly LinuxMetricsProvider _metrics;

    public MetricsCollectorService(
        ILogger<MetricsCollectorService> log,
        IServiceProvider provider,
        LinuxMetricsProvider metrics)
    {
        _log = log;
        _provider = provider;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostname = Environment.MachineName;
        while (!stoppingToken.IsCancellationRequested)
        {
            var record = _metrics.GetCurrent(hostname);
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MetricsDbContext>();
            await db.Metrics.AddAsync(record, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);
            _log.LogInformation("Saved metrics: {@Record}", record);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
