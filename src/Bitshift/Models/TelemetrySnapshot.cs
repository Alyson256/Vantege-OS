namespace Bitshift.Models
{
    public class TelemetrySnapshot
    {
        // CPU Telemetry
        public double CpuUsage { get; set; }
        public string CpuModel { get; set; } = string.Empty;

        // GPU Telemetry
        public double GpuUsage { get; set; }
        public string GpuModel { get; set; } = string.Empty;
        public double GpuVramUsedGB { get; set; }
        public double GpuVramTotalGB { get; set; }

        // RAM Telemetry
        public double RamUsagePercentage { get; set; }
        public double RamUsedGB { get; set; }
        public double RamTotalGB { get; set; }

        // Disk Telemetry
        public double DiskActiveTime { get; set; }
    }
}
