using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bitshift.Models
{
    public sealed class Win32CpuSnapshotDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "—";

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = "—";

        [JsonPropertyName("architecture")]
        public string Architecture { get; set; } = "—";

        [JsonPropertyName("physicalCores")]
        public int? PhysicalCores { get; set; }

        [JsonPropertyName("logicalThreads")]
        public int? LogicalThreads { get; set; }

        [JsonPropertyName("baseClockGhz")]
        public double? BaseClockGhz { get; set; }

        [JsonPropertyName("boostClockGhz")]
        public double? BoostClockGhz { get; set; }

        [JsonPropertyName("cacheSummary")]
        public string CacheSummary { get; set; } = "—";

        [JsonPropertyName("socket")]
        public string Socket { get; set; } = "—";

        [JsonPropertyName("virtualizationEnabled")]
        public bool? VirtualizationEnabled { get; set; }

        [JsonPropertyName("usagePercent")]
        public double? UsagePercent { get; set; }
    }

    public sealed class Win32GpuSnapshotDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "—";

        [JsonPropertyName("manufacturerInterface")]
        public string ManufacturerInterface { get; set; } = "—";

        [JsonPropertyName("vramTotalGb")]
        public double? VramTotalGb { get; set; }

        [JsonPropertyName("vramUsedGb")]
        public double? VramUsedGb { get; set; }

        [JsonPropertyName("vramBusSummary")]
        public string VramBusSummary { get; set; } = "—";

        [JsonPropertyName("baseClockMhz")]
        public double? BaseClockMhz { get; set; }

        [JsonPropertyName("boostClockMhz")]
        public double? BoostClockMhz { get; set; }

        [JsonPropertyName("driverVersion")]
        public string DriverVersion { get; set; } = "—";

        [JsonPropertyName("resolutionRefresh")]
        public string ResolutionRefresh { get; set; } = "—";

        [JsonPropertyName("usagePercent")]
        public double? UsagePercent { get; set; }
    }

    public sealed class Win32RamSnapshotDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "—";

        [JsonPropertyName("totalGb")]
        public double? TotalGb { get; set; }

        [JsonPropertyName("usableGb")]
        public double? UsableGb { get; set; }

        [JsonPropertyName("usedGb")]
        public double? UsedGb { get; set; }

        [JsonPropertyName("freeGb")]
        public double? FreeGb { get; set; }

        [JsonPropertyName("usagePercent")]
        public double? UsagePercent { get; set; }

        [JsonPropertyName("memoryBus")]
        public string MemoryBus { get; set; } = "—";

        [JsonPropertyName("configuredSpeed")]
        public string ConfiguredSpeed { get; set; } = "—";

        [JsonPropertyName("slotsUsed")]
        public int? SlotsUsed { get; set; }

        [JsonPropertyName("slotsTotal")]
        public int? SlotsTotal { get; set; }

        [JsonPropertyName("installedModules")]
        public string InstalledModules { get; set; } = "—";

        [JsonPropertyName("partNumber")]
        public string PartNumber { get; set; } = "—";
    }

    public sealed class Win32StorageDriveDto
    {
        [JsonPropertyName("letter")]
        public string Letter { get; set; } = string.Empty;

        [JsonPropertyName("volumeLabel")]
        public string? VolumeLabel { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = "—";

        [JsonPropertyName("interfaceSpec")]
        public string InterfaceSpec { get; set; } = "—";

        [JsonPropertyName("usedGb")]
        public double UsedGb { get; set; }

        [JsonPropertyName("totalGb")]
        public double TotalGb { get; set; }

        [JsonPropertyName("freeGb")]
        public double FreeGb { get; set; }

        [JsonPropertyName("percent")]
        public double Percent { get; set; }
    }

    public sealed class Win32HostMetaDto
    {
        [JsonPropertyName("computerName")]
        public string ComputerName { get; set; } = "—";

        [JsonPropertyName("osEdition")]
        public string OsEdition { get; set; } = "—";

        [JsonPropertyName("bootMode")]
        public string BootMode { get; set; } = "—";

        [JsonPropertyName("motherboard")]
        public string Motherboard { get; set; } = "—";

        [JsonPropertyName("biosVersion")]
        public string BiosVersion { get; set; } = "—";

        [JsonPropertyName("networkAdapter")]
        public string NetworkAdapter { get; set; } = "—";
    }

    public sealed class Win32HardwareSnapshotDto
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; } = true;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

        [JsonPropertyName("cpu")]
        public Win32CpuSnapshotDto Cpu { get; set; } = new();

        [JsonPropertyName("gpu")]
        public Win32GpuSnapshotDto Gpu { get; set; } = new();

        [JsonPropertyName("ram")]
        public Win32RamSnapshotDto Ram { get; set; } = new();

        [JsonPropertyName("drives")]
        public List<Win32StorageDriveDto> Drives { get; set; } = new();

        [JsonPropertyName("host")]
        public Win32HostMetaDto Host { get; set; } = new();
    }

    public sealed class CleanerExecutionResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("freedMb")]
        public double FreedMb { get; set; }

        [JsonPropertyName("deletedFilesCount")]
        public int DeletedFilesCount { get; set; }

        [JsonPropertyName("scannedSizesMb")]
        public Dictionary<string, double> ScannedSizesMb { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    }

    public sealed class PowerProfileExecutionResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; } = "balanced";

        [JsonPropertyName("activeGuid")]
        public string ActiveGuid { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public sealed class RamOptimizationResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("freedMb")]
        public double FreedMb { get; set; }

        [JsonPropertyName("trimmedProcesses")]
        public int TrimmedProcesses { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    }
}
