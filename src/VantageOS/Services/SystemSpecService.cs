using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VantageOS.Models;

namespace VantageOS.Services
{
    public interface ISystemSpecService
    {
        Task<SystemSpecInfo> GetSpecsAsync();
    }

    public class SystemSpecService : ISystemSpecService
    {
        public Task<SystemSpecInfo> GetSpecsAsync()
        {
            return Task.Run(() =>
            {
                var info = new SystemSpecInfo();

                // ── CPU ──
                var cpuItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L3CacheSize FROM Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        cpuItems.Add(new SpecItem { Label = "lblModel", Value = obj["Name"]?.ToString()?.Trim() ?? "—" });
                        cpuItems.Add(new SpecItem { Label = "lblCores", Value = $"{obj["NumberOfCores"]}C / {obj["NumberOfLogicalProcessors"]}T" });

                        var mhz = Convert.ToInt32(obj["MaxClockSpeed"] ?? 0);
                        cpuItems.Add(new SpecItem { Label = "lblBaseClock", Value = $"{mhz / 1000.0:F2} GHz" });
                        cpuItems.Add(new SpecItem { Label = "lblBoostClock", Value = $"{mhz / 1000.0:F2} GHz" }); // Same as Max on WMI

                        var l3 = Convert.ToInt64(obj["L3CacheSize"] ?? 0);
                        cpuItems.Add(new SpecItem { Label = "lblCacheL3", Value = l3 > 0 ? $"{l3 / 1024} MB" : "—" });
                        break;
                    }
                }
                catch { cpuItems.Add(new SpecItem { Label = "lblModel", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "processor", ColorKey = "Blue500Brush", Items = cpuItems });

                // ── GPU ──
                var gpuItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController");
                    foreach (var obj in searcher.Get())
                    {
                        gpuItems.Add(new SpecItem { Label = "lblModel", Value = obj["Name"]?.ToString() ?? "—" });

                        var vram = Convert.ToInt64(obj["AdapterRAM"] ?? 0);
                        gpuItems.Add(new SpecItem { Label = "lblVram", Value = vram > 0 ? $"{vram / 1024 / 1024 / 1024} GB" : "—" });

                        gpuItems.Add(new SpecItem { Label = "lblDriver", Value = obj["DriverVersion"]?.ToString() ?? "—" });
                        gpuItems.Add(new SpecItem { Label = "lblDirectx", Value = "12 (Feature Level)" });
                        break;
                    }
                }
                catch { gpuItems.Add(new SpecItem { Label = "lblModel", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "graphics", ColorKey = "Rose500Brush", Items = gpuItems });

                // ── Motherboard ──
                var mbItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                    foreach (var obj in searcher.Get())
                    {
                        mbItems.Add(new SpecItem { Label = "lblManufacturer", Value = obj["Manufacturer"]?.ToString() ?? "—" });
                        mbItems.Add(new SpecItem { Label = "lblModel", Value = obj["Product"]?.ToString() ?? "—" });
                        break;
                    }

                    using var biosSearcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS");
                    foreach (var obj in biosSearcher.Get())
                    {
                        mbItems.Add(new SpecItem { Label = "lblBiosVersion", Value = obj["SMBIOSBIOSVersion"]?.ToString() ?? "—" });
                        break;
                    }
                }
                catch { mbItems.Add(new SpecItem { Label = "lblManufacturer", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "motherboard", ColorKey = "Emerald500Brush", Items = mbItems });

                // ── Memory ──
                var ramItems = new List<SpecItem>();
                try
                {
                    long totalCapacity = 0;
                    string speed = "—";
                    string type = "—";
                    int slotsUsed = 0;

                    using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, SMBIOSMemoryType FROM Win32_PhysicalMemory");
                    foreach (var obj in searcher.Get())
                    {
                        totalCapacity += Convert.ToInt64(obj["Capacity"] ?? 0);
                        speed = $"{obj["Speed"]} MHz";
                        var memType = Convert.ToInt32(obj["SMBIOSMemoryType"] ?? 0);
                        type = memType switch
                        {
                            26 => "DDR4",
                            34 => "DDR5",
                            24 => "DDR3",
                            _ => $"Type {memType}"
                        };
                        slotsUsed++;
                    }

                    ramItems.Add(new SpecItem { Label = "lblTotalCapacity", Value = $"{totalCapacity / 1024 / 1024 / 1024} GB" });
                    ramItems.Add(new SpecItem { Label = "lblSpeed", Value = speed });
                    ramItems.Add(new SpecItem { Label = "lblType", Value = type });
                    ramItems.Add(new SpecItem { Label = "lblSlotsUsed", Value = $"{slotsUsed}" });
                }
                catch { ramItems.Add(new SpecItem { Label = "lblTotalCapacity", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "memory", ColorKey = "Purple500Brush", Items = ramItems });

                // ── Storage ──
                var storageItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Model, Size, MediaType FROM Win32_DiskDrive");
                    int diskNum = 0;
                    foreach (var obj in searcher.Get())
                    {
                        diskNum++;
                        var size = Convert.ToInt64(obj["Size"] ?? 0);
                        var sizeGB = size / 1024 / 1024 / 1024;
                        storageItems.Add(new SpecItem
                        {
                            Label = $"Disco {diskNum}",
                            Value = $"{obj["Model"]} ({sizeGB} GB)"
                        });
                        if (diskNum >= 3) break; // Max 3 disks
                    }

                    using var partSearcher = new ManagementObjectSearcher("SELECT Type FROM Win32_DiskPartition WHERE DiskIndex=0 AND Index=0");
                    foreach (var obj in partSearcher.Get())
                    {
                        storageItems.Add(new SpecItem { Label = "lblPartitionType", Value = obj["Type"]?.ToString() ?? "—" });
                        break;
                    }
                }
                catch { storageItems.Add(new SpecItem { Label = "Disco 1", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "storage", ColorKey = "Amber500Brush", Items = storageItems });

                // ── OS ──
                var osItems = new List<SpecItem>();
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem");
                    foreach (var obj in searcher.Get())
                    {
                        osItems.Add(new SpecItem { Label = "lblEdition", Value = obj["Caption"]?.ToString() ?? "—" });
                        osItems.Add(new SpecItem { Label = "lblVersion", Value = obj["Version"]?.ToString() ?? "—" });
                        osItems.Add(new SpecItem { Label = "lblArchitecture", Value = obj["OSArchitecture"]?.ToString() ?? "—" });
                        break;
                    }

                    // Secure Boot check via registry
                    try
                    {
                        var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                        if (key != null)
                        {
                            var val = key.GetValue("UEFISecureBootEnabled");
                            osItems.Add(new SpecItem { Label = "lblSecureBoot", Value = val?.ToString() == "1" ? "Ativo" : "Inativo" });
                        }
                        else
                        {
                            osItems.Add(new SpecItem { Label = "lblSecureBoot", Value = "—" });
                        }
                    }
                    catch { osItems.Add(new SpecItem { Label = "lblSecureBoot", Value = "—" }); }
                }
                catch { osItems.Add(new SpecItem { Label = "lblEdition", Value = "—" }); }

                info.Categories.Add(new SpecCategory { Name = "os", ColorKey = "Cyan500Brush", Items = osItems });

                return info;
            });
        }
    }
}
