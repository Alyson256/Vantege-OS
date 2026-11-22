using System;
using System.Collections.Generic;
using Bitshift.Models;

namespace Bitshift.Services.Mocks
{
    internal static class MockHardwareData
    {
        public static TelemetrySnapshot CreateTelemetry(bool empty = false)
        {
            if (empty) return new TelemetrySnapshot();

            return new TelemetrySnapshot
            {
                CpuUsage = 23.4,
                CpuModel = "Mock Ryzen 7 7700",
                GpuUsage = 17.2,
                GpuModel = "Mock Radeon RX 7800 XT",
                GpuVramUsedGB = 3.8,
                GpuVramTotalGB = 16,
                RamUsagePercentage = 48,
                RamUsedGB = 15.4,
                RamTotalGB = 32,
                DiskActiveTime = 8.5
            };
        }

        public static SystemSpecInfo CreateSpecs(bool empty = false)
        {
            var result = new SystemSpecInfo();
            if (empty) return result;

            result.Categories.Add(new SpecCategory
            {
                Name = "processor",
                ColorKey = "Blue500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblModel", Value = "Mock Ryzen 7 7700" },
                    new() { Label = "lblManufacturer", Value = "Mock AMD" },
                    new() { Label = "lblCores", Value = "8C / 16T" },
                    new() { Label = "lblBaseClock", Value = "3.80 GHz" },
                    new() { Label = "lblBoostClock", Value = "5.30 GHz" },
                    new() { Label = "lblCacheL3", Value = "32 MB" },
                    new() { Label = "lblSocket", Value = "AM5" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "graphics",
                ColorKey = "Rose500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblModel", Value = "Mock Radeon RX 7800 XT" },
                    new() { Label = "lblVram", Value = "16.0 GB" },
                    new() { Label = "lblDriver", Value = "Mock 24.1.1" },
                    new() { Label = "lblDirectx", Value = "DirectX 12" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "motherboard",
                ColorKey = "Emerald500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblManufacturer", Value = "Mock Systems" },
                    new() { Label = "lblModel", Value = "Mock B650" },
                    new() { Label = "lblBiosVersion", Value = "Mock 1.0" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "memory",
                ColorKey = "Purple500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblTotalCapacity", Value = "32 GB" },
                    new() { Label = "lblSpeed", Value = "6000 MT/s" },
                    new() { Label = "lblType", Value = "DDR5" },
                    new() { Label = "lblSlotsUsed", Value = "2 / 4" },
                    new() { Label = "lblPartNumber", Value = "MOCK-32G-DDR5" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "storage",
                ColorKey = "Amber500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblDisk1", Value = "256 / 1000 GB (NTFS)" },
                    new() { Label = "Physical #1", Value = "Mock NVMe SSD (1000 GB · NVMe)" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "os",
                ColorKey = "Cyan500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblEdition", Value = "Mock Windows 11 Pro" },
                    new() { Label = "lblVersion", Value = "Mock 10.0.22631" },
                    new() { Label = "lblArchitecture", Value = "64-bit" },
                    new() { Label = "lblSecureBoot", Value = "Ativo" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "technologies",
                ColorKey = "Teal500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "TPM 2.0", Value = "Ativo (2.0)" },
                    new() { Label = "HAGS (GPU Scheduling)", Value = "Ativo" },
                    new() { Label = "Virtualization", Value = "Ativo" },
                    new() { Label = "UEFI Secure Boot", Value = "Ativo" }
                }
            });
            result.Categories.Add(new SpecCategory
            {
                Name = "displayNetwork",
                ColorKey = "Indigo500Brush",
                Items = new List<SpecItem>
                {
                    new() { Label = "lblResolution", Value = "2560x1440 @ 144 Hz" },
                    new() { Label = "lblNetworkAdapter", Value = "Mock Ethernet" },
                    new() { Label = "lblNetworkLink", Value = "1 Gbps" }
                }
            });

            return result;
        }

        public static Win32HardwareSnapshotDto CreateSnapshot(bool empty = false)
        {
            if (empty)
            {
                return new Win32HardwareSnapshotDto
                {
                    Connected = false,
                    Cpu = new Win32CpuSnapshotDto(),
                    Gpu = new Win32GpuSnapshotDto(),
                    Ram = new Win32RamSnapshotDto(),
                    Drives = new List<Win32StorageDriveDto>(),
                    Host = new Win32HostMetaDto()
                };
            }

            return new Win32HardwareSnapshotDto
            {
                Connected = true,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Cpu = new Win32CpuSnapshotDto
                {
                    Model = "Mock Ryzen 7 7700",
                    Manufacturer = "Mock AMD",
                    Architecture = "x64",
                    PhysicalCores = 8,
                    LogicalThreads = 16,
                    BaseClockGhz = 3.8,
                    BoostClockGhz = 5.3,
                    CacheSummary = "32 MB L3",
                    Socket = "AM5",
                    VirtualizationEnabled = true,
                    UsagePercent = 23.4
                },
                Gpu = new Win32GpuSnapshotDto
                {
                    Model = "Mock Radeon RX 7800 XT",
                    ManufacturerInterface = "Mock AMD · PCIe",
                    VramTotalGb = 16,
                    VramUsedGb = 3.8,
                    VramBusSummary = "256-bit",
                    DriverVersion = "Mock 24.1.1",
                    ResolutionRefresh = "2560x1440 @ 144 Hz",
                    UsagePercent = 17.2
                },
                Ram = new Win32RamSnapshotDto
                {
                    Model = "Mock DDR5",
                    TotalGb = 32,
                    UsableGb = 31.8,
                    UsedGb = 15.4,
                    FreeGb = 16.4,
                    UsagePercent = 48,
                    MemoryBus = "DDR5",
                    ConfiguredSpeed = "6000 MT/s",
                    SlotsUsed = 2,
                    SlotsTotal = 4,
                    InstalledModules = "2 × 16 GB",
                    PartNumber = "MOCK-32G-DDR5"
                },
                Drives = new List<Win32StorageDriveDto>
                {
                    new()
                    {
                        Letter = "C:",
                        VolumeLabel = "Mock System",
                        Model = "Mock NVMe SSD",
                        InterfaceSpec = "NVMe",
                        UsedGb = 256,
                        TotalGb = 1000,
                        FreeGb = 744,
                        Percent = 25.6
                    }
                },
                Host = new Win32HostMetaDto
                {
                    ComputerName = "BITSHIFT-MOCK",
                    OsEdition = "Mock Windows 11 Pro",
                    BootMode = "UEFI",
                    Motherboard = "Mock B650",
                    BiosVersion = "Mock 1.0",
                    NetworkAdapter = "Mock Ethernet (1000 Mbps)"
                }
            };
        }
    }
}
