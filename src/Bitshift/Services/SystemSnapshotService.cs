using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;
using Microsoft.Win32;

namespace Bitshift.Services
{
    public sealed class SystemSnapshotService : ISystemSnapshotService
    {
        private readonly ITelemetryService _telemetryService;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private StaticHardwareCache? _staticCache;

        private sealed class StaticHardwareCache
        {
            public Win32CpuSnapshotDto Cpu { get; init; } = new();
            public Win32GpuSnapshotDto Gpu { get; init; } = new();
            public Win32RamSnapshotDto Ram { get; init; } = new();
            public Win32HostMetaDto Host { get; init; } = new();
            public Dictionary<string, (string Model, string InterfaceSpec)> DrivePhysicalMap { get; init; } =
                new(StringComparer.OrdinalIgnoreCase);
        }

        public SystemSnapshotService(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        public async Task RefreshStaticMetadataAsync(CancellationToken cancellationToken = default)
        {
            await _initLock.WaitAsync(cancellationToken);
            try
            {
                _staticCache = await Task.Run(CollectStaticHardwareMetadata, cancellationToken);
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<Win32HardwareSnapshotDto> GetHardwareSnapshotAsync(CancellationToken cancellationToken = default)
        {
            if (_staticCache == null)
            {
                await RefreshStaticMetadataAsync(cancellationToken);
            }

            var staticMeta = _staticCache ?? new StaticHardwareCache();
            var live = await _telemetryService.GetSnapshotAsync();

            var cpu = new Win32CpuSnapshotDto
            {
                Model = !string.IsNullOrWhiteSpace(staticMeta.Cpu.Model) && staticMeta.Cpu.Model != "—"
                    ? staticMeta.Cpu.Model
                    : (!string.IsNullOrWhiteSpace(live.CpuModel) ? live.CpuModel : "—"),
                Manufacturer = staticMeta.Cpu.Manufacturer,
                Architecture = staticMeta.Cpu.Architecture,
                PhysicalCores = staticMeta.Cpu.PhysicalCores,
                LogicalThreads = staticMeta.Cpu.LogicalThreads,
                BaseClockGhz = staticMeta.Cpu.BaseClockGhz,
                BoostClockGhz = staticMeta.Cpu.BoostClockGhz,
                CacheSummary = staticMeta.Cpu.CacheSummary,
                Socket = staticMeta.Cpu.Socket,
                VirtualizationEnabled = staticMeta.Cpu.VirtualizationEnabled,
                UsagePercent = Math.Round(live.CpuUsage, 1)
            };

            double? vramTotal = staticMeta.Gpu.VramTotalGb ?? (live.GpuVramTotalGB > 0 ? live.GpuVramTotalGB : null);
            var gpu = new Win32GpuSnapshotDto
            {
                Model = !string.IsNullOrWhiteSpace(staticMeta.Gpu.Model) && staticMeta.Gpu.Model != "—"
                    ? staticMeta.Gpu.Model
                    : (!string.IsNullOrWhiteSpace(live.GpuModel) ? live.GpuModel : "—"),
                ManufacturerInterface = staticMeta.Gpu.ManufacturerInterface,
                VramTotalGb = vramTotal,
                VramUsedGb = live.GpuVramUsedGB > 0 ? Math.Round(live.GpuVramUsedGB, 2) : null,
                VramBusSummary = staticMeta.Gpu.VramBusSummary,
                BaseClockMhz = staticMeta.Gpu.BaseClockMhz,
                BoostClockMhz = staticMeta.Gpu.BoostClockMhz,
                DriverVersion = staticMeta.Gpu.DriverVersion,
                ResolutionRefresh = staticMeta.Gpu.ResolutionRefresh,
                UsagePercent = Math.Round(live.GpuUsage, 1)
            };

            double? usableGb = live.RamTotalGB > 0 ? Math.Round(live.RamTotalGB, 2) : staticMeta.Ram.UsableGb;
            double? usedGb = live.RamTotalGB > 0 ? Math.Round(live.RamUsedGB, 2) : null;
            double? freeGb = live.RamTotalGB > 0 ? Math.Round(Math.Max(0.0, live.RamTotalGB - live.RamUsedGB), 2) : null;
            double? ramPercent = live.RamTotalGB > 0 ? Math.Round(live.RamUsagePercentage, 1) : null;

            var ram = new Win32RamSnapshotDto
            {
                Model = staticMeta.Ram.Model,
                TotalGb = staticMeta.Ram.TotalGb ?? usableGb,
                UsableGb = usableGb,
                UsedGb = usedGb,
                FreeGb = freeGb,
                UsagePercent = ramPercent,
                MemoryBus = staticMeta.Ram.MemoryBus,
                ConfiguredSpeed = staticMeta.Ram.ConfiguredSpeed,
                SlotsUsed = staticMeta.Ram.SlotsUsed,
                SlotsTotal = staticMeta.Ram.SlotsTotal,
                InstalledModules = staticMeta.Ram.InstalledModules,
                PartNumber = staticMeta.Ram.PartNumber
            };

            var drives = CollectDriveSnapshots(staticMeta.DrivePhysicalMap);

            return new Win32HardwareSnapshotDto
            {
                Connected = true,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Cpu = cpu,
                Gpu = gpu,
                Ram = ram,
                Drives = drives,
                Host = staticMeta.Host
            };
        }

        private static StaticHardwareCache CollectStaticHardwareMetadata()
        {
            return new StaticHardwareCache
            {
                Cpu = CollectCpuStatic(),
                Gpu = CollectGpuStatic(),
                Ram = CollectRamStatic(),
                Host = CollectHostStatic(),
                DrivePhysicalMap = CollectLogicalToPhysicalDriveMap()
            };
        }

        private static Win32CpuSnapshotDto CollectCpuStatic()
        {
            var dto = new Win32CpuSnapshotDto
            {
                Architecture = $"{RuntimeInformation.OSArchitecture} ({(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")})"
            };

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L2CacheSize, L3CacheSize, SocketDesignation, VirtualizationFirmwareEnabled FROM Win32_Processor");

                foreach (ManagementObject obj in searcher.Get())
                {
                    using (obj)
                    {
                        dto.Model = obj["Name"]?.ToString()?.Trim() ?? "—";
                        dto.Manufacturer = NormalizeCpuVendor(obj["Manufacturer"]?.ToString()?.Trim());
                        dto.PhysicalCores = TryToInt(obj["NumberOfCores"]);
                        dto.LogicalThreads = TryToInt(obj["NumberOfLogicalProcessors"]);

                        int? maxMhz = TryToInt(obj["MaxClockSpeed"]);
                        if (maxMhz is > 0)
                        {
                            double baseGhz = Math.Round(maxMhz.Value / 1000.0, 2);
                            dto.BaseClockGhz = baseGhz;
                            dto.BoostClockGhz = baseGhz;
                        }

                        long l2Kb = TryToLong(obj["L2CacheSize"]) ?? 0;
                        long l3Kb = TryToLong(obj["L3CacheSize"]) ?? 0;
                        var cacheParts = new List<string>();
                        if (l2Kb > 0) cacheParts.Add($"L2 {FormatCacheKb(l2Kb)}");
                        if (l3Kb > 0) cacheParts.Add($"L3 {FormatCacheKb(l3Kb)}");
                        dto.CacheSummary = cacheParts.Count > 0 ? string.Join(" / ", cacheParts) : "—";

                        string? socket = obj["SocketDesignation"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(socket))
                        {
                            dto.Socket = socket;
                        }

                        if (obj["VirtualizationFirmwareEnabled"] is bool virt)
                        {
                            dto.VirtualizationEnabled = virt;
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] CPU WMI query error: {ex.Message}");
            }

            return dto;
        }

        private static Win32GpuSnapshotDto CollectGpuStatic()
        {
            var dto = new Win32GpuSnapshotDto();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterCompatibility, AdapterRAM, DriverVersion, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, PNPDeviceID, VideoMemoryType FROM Win32_VideoController");

                var candidates = new List<(
                    string Name,
                    string Compatibility,
                    ulong AdapterRam,
                    string DriverVersion,
                    string Resolution,
                    string PnpId,
                    int VideoMemType,
                    bool IsSoftwareAdapter)>();

                foreach (ManagementObject obj in searcher.Get())
                {
                    using (obj)
                    {
                        string name = obj["Name"]?.ToString()?.Trim() ?? "—";
                        string compat = obj["AdapterCompatibility"]?.ToString()?.Trim() ?? string.Empty;
                        ulong adapterRam = TryToULong(obj["AdapterRAM"]) ?? 0;
                        string driver = obj["DriverVersion"]?.ToString()?.Trim() ?? "—";
                        string pnp = obj["PNPDeviceID"]?.ToString()?.Trim() ?? string.Empty;
                        int vramType = TryToInt(obj["VideoMemoryType"]) ?? 0;

                        string? hRes = obj["CurrentHorizontalResolution"]?.ToString();
                        string? vRes = obj["CurrentVerticalResolution"]?.ToString();
                        string? hz = obj["CurrentRefreshRate"]?.ToString();
                        string resolution = !string.IsNullOrEmpty(hRes) && !string.IsNullOrEmpty(vRes)
                            ? (!string.IsNullOrEmpty(hz) ? $"{hRes}x{vRes} @ {hz} Hz" : $"{hRes}x{vRes}")
                            : string.Empty;

                        bool isSoftware = Win32HardwareHelpers.IsSoftwareGpuAdapter(name);

                        candidates.Add((name, compat, adapterRam, driver, resolution, pnp, vramType, isSoftware));
                    }
                }

                if (candidates.Count > 0)
                {
                    var selected = candidates
                        .OrderBy(c => c.IsSoftwareAdapter ? 1 : 0)
                        .ThenByDescending(c => Win32HardwareHelpers.ReadGpuMemoryBytesFromRegistry(c.Name))
                        .ThenByDescending(c => c.AdapterRam)
                        .First();

                    dto.Model = selected.Name;
                    dto.DriverVersion = selected.DriverVersion;

                    string busType = selected.PnpId.StartsWith("PCI\\", StringComparison.OrdinalIgnoreCase)
                        ? "PCIe"
                        : "Integrated / Display";
                    dto.ManufacturerInterface = !string.IsNullOrWhiteSpace(selected.Compatibility)
                        ? $"{selected.Compatibility} · {busType}"
                        : busType;

                    ulong regVram = Win32HardwareHelpers.ReadGpuMemoryBytesFromRegistry(selected.Name);
                    ulong vramBytes = regVram > 0 ? regVram : selected.AdapterRam;
                    if (vramBytes > 0)
                    {
                        double vramGb = Math.Round(vramBytes / 1024.0 / 1024.0 / 1024.0, 1);
                        dto.VramTotalGb = vramGb;
                        dto.VramBusSummary = $"{vramGb:F1} GB Dedicated";
                    }

                    string bestResolution = !string.IsNullOrWhiteSpace(selected.Resolution)
                        ? selected.Resolution
                        : candidates.Select(c => c.Resolution).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r)) ?? "—";
                    dto.ResolutionRefresh = bestResolution;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] GPU WMI query error: {ex.Message}");
            }

            return dto;
        }

        private static Win32RamSnapshotDto CollectRamStatic()
        {
            var dto = new Win32RamSnapshotDto();

            try
            {
                int slotsTotal = 0;
                using (var arraySearcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"))
                {
                    foreach (ManagementObject obj in arraySearcher.Get())
                    {
                        using (obj)
                        {
                            slotsTotal += TryToInt(obj["MemoryDevices"]) ?? 0;
                        }
                    }
                }

                long totalBytes = 0;
                int slotsUsed = 0;
                int speedMt = 0;
                string ddrType = "—";
                var moduleSizesGb = new List<int>();
                var manufacturers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var partNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using var searcher = new ManagementObjectSearcher(
                    "SELECT Capacity, Speed, ConfiguredClockSpeed, SMBIOSMemoryType, Manufacturer, PartNumber FROM Win32_PhysicalMemory");

                foreach (ManagementObject obj in searcher.Get())
                {
                    using (obj)
                    {
                        slotsUsed++;
                        long cap = TryToLong(obj["Capacity"]) ?? 0;
                        totalBytes += cap;
                        if (cap > 0)
                        {
                            moduleSizesGb.Add((int)Math.Round(cap / 1024.0 / 1024.0 / 1024.0));
                        }

                        int rawSpeed = TryToInt(obj["ConfiguredClockSpeed"]) ?? TryToInt(obj["Speed"]) ?? 0;
                        if (rawSpeed > 0) speedMt = rawSpeed;

                        int memType = TryToInt(obj["SMBIOSMemoryType"]) ?? 0;
                        ddrType = memType switch
                        {
                            24 => "DDR3",
                            26 => "DDR4",
                            30 => "LPDDR4",
                            34 => "DDR5",
                            35 => "LPDDR5",
                            _ => memType > 0 ? $"SMBIOS {memType}" : ddrType
                        };

                        string? mfg = obj["Manufacturer"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(mfg) && !mfg.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        {
                            manufacturers.Add(mfg);
                        }

                        string? pn = obj["PartNumber"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(pn) && !pn.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        {
                            partNumbers.Add(pn);
                        }
                    }
                }

                if (totalBytes > 0)
                {
                    dto.TotalGb = Math.Round(totalBytes / 1024.0 / 1024.0 / 1024.0, 1);
                }

                dto.SlotsUsed = slotsUsed > 0 ? slotsUsed : null;
                dto.SlotsTotal = slotsTotal > 0 ? slotsTotal : null;
                dto.ConfiguredSpeed = speedMt > 0 ? $"{speedMt} MT/s" : "—";

                string channelText = slotsUsed switch
                {
                    >= 4 => "Quad Channel",
                    >= 2 => "Dual Channel",
                    1 => "Single Channel",
                    _ => string.Empty
                };
                dto.MemoryBus = !string.IsNullOrEmpty(channelText) && ddrType != "—"
                    ? $"{ddrType} · {channelText}"
                    : ddrType;

                if (moduleSizesGb.Count > 0)
                {
                    dto.InstalledModules = moduleSizesGb.Distinct().Count() == 1
                        ? $"{moduleSizesGb.Count}x {moduleSizesGb[0]} GB"
                        : string.Join(" + ", moduleSizesGb.Select(s => $"{s} GB"));
                }

                dto.PartNumber = partNumbers.Count > 0 ? string.Join(" / ", partNumbers) : "—";

                string vendor = manufacturers.Count > 0 ? string.Join(" / ", manufacturers) : string.Empty;
                dto.Model = !string.IsNullOrWhiteSpace(vendor)
                    ? $"{vendor} {ddrType}".Trim()
                    : (ddrType != "—" ? $"{ddrType} System Memory" : "System Memory");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] RAM WMI query error: {ex.Message}");
            }

            return dto;
        }

        private static Win32HostMetaDto CollectHostStatic()
        {
            var host = new Win32HostMetaDto
            {
                ComputerName = Environment.MachineName
            };

            try
            {
                using var osSearcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in osSearcher.Get())
                {
                    using (obj)
                    {
                        string caption = obj["Caption"]?.ToString()?.Trim() ?? "Windows";
                        string build = obj["BuildNumber"]?.ToString()?.Trim() ?? string.Empty;
                        host.OsEdition = !string.IsNullOrEmpty(build) ? $"{caption} (Build {build})" : caption;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] OS WMI query error: {ex.Message}");
            }

            try
            {
                using var mbSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                foreach (ManagementObject obj in mbSearcher.Get())
                {
                    using (obj)
                    {
                        string mfg = obj["Manufacturer"]?.ToString()?.Trim() ?? string.Empty;
                        string product = obj["Product"]?.ToString()?.Trim() ?? string.Empty;
                        host.Motherboard = $"{mfg} {product}".Trim();
                        if (string.IsNullOrWhiteSpace(host.Motherboard)) host.Motherboard = "—";
                        break;
                    }
                }

                using var biosSearcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS");
                foreach (ManagementObject obj in biosSearcher.Get())
                {
                    using (obj)
                    {
                        host.BiosVersion = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? "—";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] Motherboard/BIOS query error: {ex.Message}");
            }

            host.BootMode = ReadBootModeSummary();
            var nic = Win32HardwareHelpers.GetActiveNetworkAdapterSummary();
            host.NetworkAdapter = nic.Mbps > 0 ? $"{nic.Name} ({nic.Mbps} Mbps)" : nic.Name;

            return host;
        }

        private static Dictionary<string, (string Model, string InterfaceSpec)> CollectLogicalToPhysicalDriveMap()
        {
            var map = new Dictionary<string, (string Model, string InterfaceSpec)>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var physicalDrives = new Dictionary<string, (string Model, string InterfaceSpec)>(StringComparer.OrdinalIgnoreCase);
                using (var driveSearcher = new ManagementObjectSearcher("SELECT DeviceID, Model, InterfaceType, MediaType FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject disk in driveSearcher.Get())
                    {
                        using (disk)
                        {
                            string id = disk["DeviceID"]?.ToString() ?? string.Empty;
                            string model = disk["Model"]?.ToString()?.Trim() ?? "Fixed Disk";
                            string iface = disk["InterfaceType"]?.ToString()?.Trim() ?? "Disk";
                            if (model.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
                            {
                                iface = "NVMe";
                            }
                            if (!string.IsNullOrEmpty(id))
                            {
                                physicalDrives[id] = (model, iface);
                            }
                        }
                    }
                }

                var partitionToDrive = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                using (var partRelSearcher = new ManagementObjectSearcher("SELECT Antecedent, Dependent FROM Win32_DiskDriveToDiskPartition"))
                {
                    foreach (ManagementObject rel in partRelSearcher.Get())
                    {
                        using (rel)
                        {
                            string? ant = ExtractWmiKey(rel["Antecedent"]?.ToString(), "DeviceID");
                            string? dep = ExtractWmiKey(rel["Dependent"]?.ToString(), "DeviceID");
                            if (!string.IsNullOrEmpty(ant) && !string.IsNullOrEmpty(dep))
                            {
                                partitionToDrive[dep] = ant.Replace("\\\\", "\\", StringComparison.Ordinal);
                            }
                        }
                    }
                }

                using var logRelSearcher = new ManagementObjectSearcher("SELECT Antecedent, Dependent FROM Win32_LogicalDiskToPartition");
                foreach (ManagementObject rel in logRelSearcher.Get())
                {
                    using (rel)
                    {
                        string? partId = ExtractWmiKey(rel["Antecedent"]?.ToString(), "DeviceID");
                        string? driveLetter = ExtractWmiKey(rel["Dependent"]?.ToString(), "DeviceID");
                        if (!string.IsNullOrEmpty(partId) && !string.IsNullOrEmpty(driveLetter) &&
                            partitionToDrive.TryGetValue(partId, out string? physId) &&
                            physicalDrives.TryGetValue(physId, out var physInfo))
                        {
                            map[driveLetter] = physInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] Drive topology mapping error: {ex.Message}");
            }

            return map;
        }

        private static List<Win32StorageDriveDto> CollectDriveSnapshots(
            IReadOnlyDictionary<string, (string Model, string InterfaceSpec)> drivePhysicalMap)
        {
            var drives = new List<Win32StorageDriveDto>();
            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
                {
                    double totalGb = Math.Round(drive.TotalSize / 1024.0 / 1024.0 / 1024.0, 1);
                    double freeGb = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0, 1);
                    double usedGb = Math.Round(Math.Max(0.0, totalGb - freeGb), 1);
                    double percent = totalGb > 0 ? Math.Round((usedGb / totalGb) * 100.0, 1) : 0.0;
                    string letter = drive.Name.TrimEnd('\\');

                    string model = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? $"Local Disk ({letter})" : drive.VolumeLabel;
                    string iface = drive.DriveFormat;

                    if (drivePhysicalMap.TryGetValue(letter, out var phys))
                    {
                        model = phys.Model;
                        iface = $"{phys.InterfaceSpec} · {drive.DriveFormat}";
                    }

                    drives.Add(new Win32StorageDriveDto
                    {
                        Letter = letter,
                        VolumeLabel = drive.VolumeLabel,
                        Model = model,
                        InterfaceSpec = iface,
                        UsedGb = usedGb,
                        TotalGb = totalGb,
                        FreeGb = freeGb,
                        Percent = percent
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] DriveInfo scan error: {ex.Message}");
            }

            return drives;
        }

        private static string ReadBootModeSummary()
        {
            try
            {
                using var sessionKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control");
                int firmwareType = TryToInt(sessionKey?.GetValue("PEFirmwareType")) ?? 0;
                string fwLabel = firmwareType == 2 ? "UEFI" : firmwareType == 1 ? "Legacy BIOS" : "UEFI/BIOS";

                using var sbKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                string? sbVal = sbKey?.GetValue("UEFISecureBootEnabled")?.ToString();
                return sbVal switch
                {
                    "1" => $"{fwLabel} (Secure Boot On)",
                    "0" => $"{fwLabel} (Secure Boot Off)",
                    _ => fwLabel
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemSnapshotService] BootMode read error: {ex.Message}");
                return "—";
            }
        }

        private static string NormalizeCpuVendor(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "—";
            return raw switch
            {
                "GenuineIntel" => "Intel",
                "AuthenticAMD" => "AMD",
                _ => raw
            };
        }

        private static string FormatCacheKb(long kb)
        {
            return kb >= 1024 ? $"{kb / 1024} MB" : $"{kb} KB";
        }

        private static string? ExtractWmiKey(string? relPath, string keyName)
        {
            if (string.IsNullOrWhiteSpace(relPath)) return null;
            int idx = relPath.IndexOf($"{keyName}=\"", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + keyName.Length + 2;
            int end = relPath.IndexOf('"', start);
            return end > start ? relPath.Substring(start, end - start) : null;
        }

        private static int? TryToInt(object? val)
        {
            if (val == null) return null;
            try { return Convert.ToInt32(val); } catch { return null; }
        }

        private static long? TryToLong(object? val)
        {
            if (val == null) return null;
            try { return Convert.ToInt64(val); } catch { return null; }
        }

        private static ulong? TryToULong(object? val)
        {
            if (val == null) return null;
            try { return Convert.ToUInt64(val); } catch { return null; }
        }
    }
}
