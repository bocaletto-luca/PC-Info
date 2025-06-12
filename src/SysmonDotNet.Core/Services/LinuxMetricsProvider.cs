using SysmonDotNet.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysmonDotNet.Core.Services
{
    public class LinuxMetricsProvider
    {
        // Snapshot struct for parsare /proc/stat
        private record CpuSnapshot(long Idle, long Total);

        // Metodo pubblico: legge e calcola CPU usage in %
        public async Task<double> ReadCpuUsageAsync(int delayMs = 100)
        {
            var snap1 = await CaptureSnapshotAsync();
            await Task.Delay(delayMs);
            var snap2 = await CaptureSnapshotAsync();

            var idleDelta = snap2.Idle - snap1.Idle;
            var totalDelta = snap2.Total - snap1.Total;
            if (totalDelta == 0) return 0;

            var usage = (1.0 - (double)idleDelta / totalDelta) * 100.0;
            return Math.Round(usage, 1);
        }

        // Cattura una istantanea di idle+total del primo core line del file
        private static async Task<CpuSnapshot> CaptureSnapshotAsync()
        {
            var line = (await File.ReadAllLinesAsync("/proc/stat"))
                       .FirstOrDefault(l => l.StartsWith("cpu "));
            if (line == null) throw new InvalidOperationException("Cannot read /proc/stat");

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Skip(1)  // skip “cpu”
                            .Select(long.Parse)
                            .ToArray();

            // fields: user, nice, system, idle, iowait, irq, softirq, steal, ...
            long idle = parts[3] + parts[4];   // idle + iowait
            long total = parts.Sum();

            return new CpuSnapshot(idle, total);
        }

        // Resto dei metodi...
        public double ReadMemoryUsedMb()
        {
            var info = File.ReadAllLines("/proc/meminfo")
                .Select(l => l.Split(':'))
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
            double total = double.Parse(info["MemTotal"].Replace(" kB",""));
            double free  = double.Parse(info["MemAvailable"].Replace(" kB",""));
            return Math.Round((total - free) / 1024.0, 1);
        }

        public double ReadDiskUsedPercent(string mountPoint)
        {
            var stat = new System.IO.DriveInfo(mountPoint);
            return Math.Round((stat.TotalSize - stat.AvailableFreeSpace) * 100.0 / stat.TotalSize, 1);
        }

        public (double rx, double tx) ReadNetworkKb(string iface)
        {
            var parts = File.ReadAllText("/proc/net/dev")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(l => l.Trim().StartsWith(iface + ":"))
                ?.Split(':')[1]
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length < 10) return (0, 0);
            double rx = double.Parse(parts[0]) / 1024.0;
            double tx = double.Parse(parts[8]) / 1024.0;
            return (Math.Round(rx,1), Math.Round(tx,1));
        }

        public MetricRecord GetCurrent(string hostname)
        {
            var now = DateTime.UtcNow;
            var cpuTask = ReadCpuUsageAsync();
            cpuTask.Wait();
            var cpu = cpuTask.Result;
            var mem = ReadMemoryUsedMb();
            var disk = ReadDiskUsedPercent("/");
            var (rx, tx) = ReadNetworkKb("eth0");

            return new MetricRecord {
                Hostname = hostname,
                Timestamp = now,
                CpuUsage = cpu,
                MemoryUsedMb = mem,
                DiskUsedPct = disk,
                NetworkRxKb = rx,
                NetworkTxKb = tx
            };
        }
    }
}
