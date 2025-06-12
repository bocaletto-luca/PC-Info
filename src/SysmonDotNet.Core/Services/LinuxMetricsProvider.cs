using SysmonDotNet.Core.Models;

namespace SysmonDotNet.Core.Services;

public class LinuxMetricsProvider
{
    public MetricRecord GetCurrent(string hostname)
    {
        var now = DateTime.UtcNow;
        double cpu = ReadCpuUsage();
        double mem = ReadMemoryUsedMb();
        double disk = ReadDiskUsedPercent("/");
        (double rx, double tx) = ReadNetworkKb("eth0");

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

    private double ReadCpuUsage() {
        var lines = File.ReadAllLines("/proc/stat");
        // parse first line for user/nice/system/idle and compute delta
        // simplified: return 12.3;
        return 12.3;
    }

    private double ReadMemoryUsedMb() {
        var info = File.ReadAllLines("/proc/meminfo")
            .Select(l => l.Split(':'))
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
        double total = double.Parse(info["MemTotal"].Replace(" kB",""));
        double free  = double.Parse(info["MemAvailable"].Replace(" kB",""));
        return (total - free) / 1024.0;
    }

    private double ReadDiskUsedPercent(string mountPoint) {
        var stat = new System.IO.DriveInfo(mountPoint);
        return (stat.TotalSize - stat.AvailableFreeSpace) * 100.0 / stat.TotalSize;
    }

    private (double rx, double tx) ReadNetworkKb(string iface) {
        var parts = File.ReadAllText("/proc/net/dev")
            .Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l=>l.Trim().StartsWith(iface+":"))
            ?.Split(':')[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts==null || parts.Length<10) return (0,0);
        double rx = double.Parse(parts[0]) / 1024.0;
        double tx = double.Parse(parts[8]) / 1024.0;
        return (rx, tx);
    }
}
