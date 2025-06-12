# PC-Info - SysmonDotNet

Cross-platform Linux service in C# .NET 7  
• System metrics (CPU, Mem, Disk, Net)  
• Records in SQLite  
• Exposes /api/metrics and /metrics (Prometheus)  
• Health check `/health`  
• Configurable interval, DI, logging, Swagger  

## Quickstart

1. Clone + build  
   dotnet build  
2. Configura in `appsettings.json`  
   DbPath = "data/metrics.db"  
3. Esegui  
   dotnet run --project src/SysmonDotNet.Api  
4. Visita  
   http://localhost/health  
   http://localhost/api/metrics  
   http://localhost/metrics  

## Docker

docker build -t sysmondotnet .  
docker run -d -p 80:80 sysmondotnet  
