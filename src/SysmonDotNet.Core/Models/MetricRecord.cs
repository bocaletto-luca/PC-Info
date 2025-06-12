namespace SysmonDotNet.Core.Models;

public class MetricRecord
{
    public int Id { get; set; }
    public string Hostname { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsedMb { get; set; }
    public double DiskUsedPct { get; set; }
    public double NetworkRxKb { get; set; }
    public double NetworkTxKb { get; set; }
}
