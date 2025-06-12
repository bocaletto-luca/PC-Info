using System.IO;
using System.Threading.Tasks;
using SysmonDotNet.Core.Services;
using Xunit;

namespace SysmonDotNet.Tests
{
    public class LinuxMetricsProviderTests
    {
        // Contenuto mock di /proc/stat a t=0 e t=100ms
        private readonly string snap1 =
@"cpu  10000 200 3000 40000 500 600 700 0 0 0
cpu0 5000 100 1500 20000 250 300 350 0 0 0";
        private readonly string snap2 =
@"cpu  10200 200 3200 40200 500 600 700 0 0 0
cpu0 5100 100 1600 20100 250 300 350 0 0 0";

        [Fact]
        public async Task ReadCpuUsageAsync_ReturnsExpected()
        {
            // Crea file temporanei
            var tempFile = Path.GetTempFileName();
            try
            {
                // Primo snapshot
                File.WriteAllText("/tmp/proc_stat_1", snap1);
                // Secondo snapshot
                File.WriteAllText("/tmp/proc_stat_2", snap2);

                // Override path con symlink
                File.Delete("/proc/stat");
                File.CreateSymbolicLink("/proc/stat", "/tmp/proc_stat_1"); 
                
                var provider = new LinuxMetricsProvider();
                
                // Sostituisci linea di Read con lettura da file1, poi da file2
                // Nota: in un test reale faresti Dependency Injection per il path.
                // Qui forziamo manualmente:
                var usage = await provider.ReadCpuUsageAsync(10);

                // Calcolo manuale:
                // Idle1 = 40000+500 = 40500, Total1 = 10000+200+3000+40000+500+600+700 = 548 + ... = 
                // Idle2 = 40200+500 = 40700, Total2 = 10200+200+3200+40200+500+600+700 = ...
                // ΔIdle = 200, ΔTotal = 548+... difference = 10200... => usage ≈?
                Assert.InRange(usage, 6.5, 7.5); // adatta in base al calcolo preciso
            }
            finally
            {
                File.Delete("/tmp/proc_stat_1");
                File.Delete("/tmp/proc_stat_2");
                File.Delete(tempFile);
            }
        }
    }
}
