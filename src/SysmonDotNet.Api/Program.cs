using Microsoft.EntityFrameworkCore;
using SysmonDotNet.Core.Services;
using SysmonDotNet.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MetricsDbContext>(opts =>
    opts.UseSqlite($"Data Source={builder.Configuration["DbPath"]}"));
builder.Services.AddSingleton<LinuxMetricsProvider>();
builder.Services.AddHostedService<MetricsCollectorService>();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(prometheus.ClientMetrics.CreateCollector());

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapControllers();
app.UseSwagger(); app.UseSwaggerUI();
app.MapMetrics(); // Prometheus endpoint
app.Run();
