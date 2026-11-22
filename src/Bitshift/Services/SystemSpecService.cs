using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;
using Microsoft.Win32;

namespace Bitshift.Services
{
    public class SystemSpecService : ISystemSpecService
    {
        public Task<SystemSpecInfo> GetSpecsAsync()
        {
            return Task.Run(() =>
            {
                var info = new SystemSpecInfo();
                bool virtualizationEnabled = false;
                string gpuResolutionRefresh = "—";

                // ── CPU ──
                var cpuItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        "SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L3CacheSize, SocketDesignation, VirtualizationFirmwareEnabled FROM Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        cpuItems.Add(new SpecItem { Label = "lblModel", Value = obj["Name"]?.ToString()?.Trim() ?? "—" });
                        cpuItems.Add(new SpecItem { Label = "lblManufacturer", Value = obj["Manufacturer"]?.ToString()?.Trim() ?? "—" });
                        cpuItems.Add(new SpecItem { Label = "lblCores", Value = $"{obj["NumberOfCores"]}C / {obj["NumberOfLogicalProcessors"]}T" });

                        var mhz = Convert.ToInt32(obj["MaxClockSpeed"] ?? 0);
                        var ghzText = mhz > 0 ? $"{mhz / 1000.0:F2} GHz" : "—";
                        cpuItems.Add(new SpecItem { Label = "lblBaseClock", Value = ghzText });
                        cpuItems.Add(new SpecItem { Label = "lblBoostClock", Value = ghzText });

                        var l3 = Convert.ToInt64(obj["L3CacheSize"] ?? 0);
                        cpuItems.Add(new SpecItem { Label = "lblCacheL3", Value = l3 > 0 ? $"{l3 / 1024} MB" : "—" });

                        var socket = obj["SocketDesignation"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(socket))
                        {
                            cpuItems.Add(new SpecItem { Label = "lblSocket", Value = socket });
                        }

                        if (obj["VirtualizationFirmwareEnabled"] is bool virt)
                        {
                            virtualizationEnabled = virt;
                        }
                        break;
                    }
                }
                catch
                {
                    cpuItems.Add(new SpecItem { Label = "lblModel", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "processor", ColorKey = "Blue500Brush", Items = cpuItems });

                // ── GPU ──
                var gpuItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        "SELECT Name, AdapterRAM, DriverVersion, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, VideoProcessor FROM Win32_VideoController");
                    foreach (var obj in searcher.Get())
                    {
                        string gpuName = obj["Name"]?.ToString()?.Trim() ?? "—";
                        gpuItems.Add(new SpecItem { Label = "lblModel", Value = gpuName });

                        ulong vramBytes = Win32HardwareHelpers.ReadGpuMemoryBytesFromRegistry(gpuName);
                        if (vramBytes == 0 && obj["AdapterRAM"] != null)
                        {
                            vramBytes = Convert.ToUInt64(obj["AdapterRAM"]);
                        }
                        gpuItems.Add(new SpecItem
                        {
                            Label = "lblVram",
                            Value = vramBytes > 0 ? $"{vramBytes / 1024.0 / 1024.0 / 1024.0:F1} GB" : "—"
                        });

                        gpuItems.Add(new SpecItem { Label = "lblDriver", Value = obj["DriverVersion"]?.ToString() ?? "—" });

                        var hRes = obj["CurrentHorizontalResolution"]?.ToString();
                        var vRes = obj["CurrentVerticalResolution"]?.ToString();
                        var hz = obj["CurrentRefreshRate"]?.ToString();
                        if (!string.IsNullOrEmpty(hRes) && !string.IsNullOrEmpty(vRes))
                        {
                            gpuResolutionRefresh = !string.IsNullOrEmpty(hz) ? $"{hRes}x{vRes} @ {hz} Hz" : $"{hRes}x{vRes}";
                        }

                        gpuItems.Add(new SpecItem { Label = "lblDirectx", Value = ReadDirectXVersionFromRegistry() });
                        break;
                    }
                }
                catch
                {
                    gpuItems.Add(new SpecItem { Label = "lblModel", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "graphics", ColorKey = "Rose500Brush", Items = gpuItems });

                // ── Motherboard ──
                var mbItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                    foreach (var obj in searcher.Get())
                    {
                        mbItems.Add(new SpecItem { Label = "lblManufacturer", Value = obj["Manufacturer"]?.ToString()?.Trim() ?? "—" });
                        mbItems.Add(new SpecItem { Label = "lblModel", Value = obj["Product"]?.ToString()?.Trim() ?? "—" });
                        break;
                    }

                    using var biosSearcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS");
                    foreach (var obj in biosSearcher.Get())
                    {
                        mbItems.Add(new SpecItem { Label = "lblBiosVersion", Value = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? "—" });
                        break;
                    }
                }
                catch
                {
                    mbItems.Add(new SpecItem { Label = "lblManufacturer", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "motherboard", ColorKey = "Emerald500Brush", Items = mbItems });

                // ── Memory ──
                var ramItems = new List<SpecItem>();
                try
                {
                    long totalCapacity = 0;
                    string speed = "—";
                    string type = "—";
                    int slotsUsed = 0;
                    int slotsTotal = 0;
                    string partNumber = "—";

                    using (var arraySearcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"))
                    {
                        foreach (var obj in arraySearcher.Get())
                        {
                            slotsTotal += Convert.ToInt32(obj["MemoryDevices"] ?? 0);
                        }
                    }

                    using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, ConfiguredClockSpeed, SMBIOSMemoryType, PartNumber FROM Win32_PhysicalMemory");
                    foreach (var obj in searcher.Get())
                    {
                        totalCapacity += Convert.ToInt64(obj["Capacity"] ?? 0);
                        var rawSpeed = obj["ConfiguredClockSpeed"] ?? obj["Speed"];
                        if (rawSpeed != null && Convert.ToInt32(rawSpeed) > 0)
                        {
                            speed = $"{rawSpeed} MT/s";
                        }

                        var memType = Convert.ToInt32(obj["SMBIOSMemoryType"] ?? 0);
                        type = memType switch
                        {
                            24 => "DDR3",
                            26 => "DDR4",
                            34 => "DDR5",
                            35 => "LPDDR5",
                            30 => "LPDDR4",
                            _ => memType > 0 ? $"SMBIOS {memType}" : "—"
                        };

                        var pn = obj["PartNumber"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(pn) && partNumber == "—")
                        {
                            partNumber = pn;
                        }
                        slotsUsed++;
                    }

                    ramItems.Add(new SpecItem
                    {
                        Label = "lblTotalCapacity",
                        Value = totalCapacity > 0 ? $"{totalCapacity / 1024 / 1024 / 1024} GB" : "—"
                    });
                    ramItems.Add(new SpecItem { Label = "lblSpeed", Value = speed });
                    ramItems.Add(new SpecItem { Label = "lblType", Value = type });
                    ramItems.Add(new SpecItem
                    {
                        Label = "lblSlotsUsed",
                        Value = slotsTotal > 0 ? $"{slotsUsed} / {slotsTotal}" : $"{slotsUsed}"
                    });
                    ramItems.Add(new SpecItem { Label = "lblPartNumber", Value = partNumber });
                }
                catch
                {
                    ramItems.Add(new SpecItem { Label = "lblTotalCapacity", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "memory", ColorKey = "Purple500Brush", Items = ramItems });

                // ── Storage (Logical Volumes + Physical Drives) ──
                var storageItems = new List<SpecItem>();
                try
                {
                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
                    {
                        double totalGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        double freeGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        double usedGb = totalGb - freeGb;
                        string letter = drive.Name.TrimEnd('\\');
                        storageItems.Add(new SpecItem
                        {
                            Label = letter == "C:" ? "lblDisk1" : letter == "D:" ? "lblDisk2" : letter,
                            Value = $"{usedGb:F0} / {totalGb:F0} GB ({drive.DriveFormat})"
                        });
                    }

                    using var searcher = new ManagementObjectSearcher("SELECT Model, InterfaceType, Size FROM Win32_DiskDrive");
                    int diskNum = 0;
                    foreach (var obj in searcher.Get())
                    {
                        diskNum++;
                        var size = Convert.ToInt64(obj["Size"] ?? 0);
                        var sizeGb = size / 1024 / 1024 / 1024;
                        storageItems.Add(new SpecItem
                        {
                            Label = $"Physical #{diskNum}",
                            Value = $"{obj["Model"]?.ToString()?.Trim()} ({sizeGb} GB · {obj["InterfaceType"]})"
                        });
                        if (diskNum >= 4) break;
                    }
                }
                catch
                {
                    storageItems.Add(new SpecItem { Label = "lblDisk1", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "storage", ColorKey = "Amber500Brush", Items = storageItems });

                // ── OS ──
                var osItems = new List<SpecItem>();
                bool secureBootActive = false;
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem");
                    foreach (var obj in searcher.Get())
                    {
                        osItems.Add(new SpecItem { Label = "lblEdition", Value = obj["Caption"]?.ToString()?.Trim() ?? "—" });
                        osItems.Add(new SpecItem { Label = "lblVersion", Value = obj["Version"]?.ToString()?.Trim() ?? "—" });
                        osItems.Add(new SpecItem { Label = "lblArchitecture", Value = obj["OSArchitecture"]?.ToString()?.Trim() ?? "—" });
                        break;
                    }

                    using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                    if (key != null)
                    {
                        var val = key.GetValue("UEFISecureBootEnabled");
                        secureBootActive = val?.ToString() == "1";
                        osItems.Add(new SpecItem { Label = "lblSecureBoot", Value = secureBootActive ? "Ativo" : "Inativo" });
                    }
                    else
                    {
                        osItems.Add(new SpecItem { Label = "lblSecureBoot", Value = "—" });
                    }
                }
                catch
                {
                    osItems.Add(new SpecItem { Label = "lblEdition", Value = "—" });
                }

                info.Categories.Add(new SpecCategory { Name = "os", ColorKey = "Cyan500Brush", Items = osItems });

                // ── Technologies & Security (Real Win32/Registry) ──
                var techItems = new List<SpecItem>
                {
                    new SpecItem { Label = "TPM 2.0", Value = ReadTpmStatus() },
                    new SpecItem { Label = "HAGS (GPU Scheduling)", Value = ReadHagsStatus() },
                    new SpecItem { Label = "Virtualization", Value = virtualizationEnabled ? "Ativo" : "Inativo" },
                    new SpecItem { Label = "UEFI Secure Boot", Value = secureBootActive ? "Ativo" : "Inativo" }
                };
                info.Categories.Add(new SpecCategory { Name = "technologies", ColorKey = "Teal500Brush", Items = techItems });

                // ── Display & Network (Real Win32/WMI/.NET) ──
                var dispNetItems = new List<SpecItem>
                {
                    new SpecItem { Label = "lblResolution", Value = gpuResolutionRefresh }
                };

                var activeNic = Win32HardwareHelpers.GetActiveNetworkAdapterSummary();
                dispNetItems.Add(new SpecItem { Label = "lblNetworkAdapter", Value = activeNic.Name });
                dispNetItems.Add(new SpecItem { Label = "lblNetworkLink", Value = activeNic.SpeedLabel });

                info.Categories.Add(new SpecCategory { Name = "displayNetwork", ColorKey = "Indigo500Brush", Items = dispNetItems });

                return info;
            });
        }

        private static string ReadDirectXVersionFromRegistry()
        {
            try
            {
                using var dxKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX");
                var ver = dxKey?.GetValue("Version")?.ToString();
                if (!string.IsNullOrWhiteSpace(ver))
                {
                    return ver.StartsWith("4.09", StringComparison.Ordinal) ? "DirectX 12" : ver;
                }
            }
            catch { }
            return "—";
        }

        private static string ReadTpmStatus()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\cimv2\security\microsofttpm", "SELECT IsActivated_InitialValue, IsEnabled_InitialValue, SpecVersion FROM Win32_Tpm");
                foreach (var obj in searcher.Get())
                {
                    bool enabled = Convert.ToBoolean(obj["IsEnabled_InitialValue"] ?? false);
                    string spec = obj["SpecVersion"]?.ToString()?.Split(',')[0]?.Trim() ?? "TPM";
                    return enabled ? $"Ativo ({spec})" : $"Inativo ({spec})";
                }
            }
            catch { }
            return "—";
        }

        private static string ReadHagsStatus()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers");
                var val = key?.GetValue("HwSchMode")?.ToString();
                return val switch
                {
                    "2" => "Ativo",
                    "1" => "Inativo",
                    _ => "—"
                };
            }
            catch
            {
                return "—";
            }
        }
    }
}
