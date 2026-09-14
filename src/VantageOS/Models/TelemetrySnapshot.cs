namespace VantageOS.Models
{
    public class TelemetrySnapshot
    {
        public double CpuUsage { get; set; }
        public string CpuModel { get; set; } = string.Empty;
        
        public double GpuUsage { get; set; }
        public string GpuModel { get; set; } = string.Empty;
        
        public double RamUsagePercentage { get; set; }
        public double RamUsedGB { get; set; }
        public double RamTotalGB { get; set; }
        
        public double DiskActiveTime { get; set; }
        
        // Emulação de DPC/ISR por agora, pois foi movido para standby
        public double DpcLatency { get; set; } = 150.0;
        public double IsrLatency { get; set; } = 50.0;
    }
}
