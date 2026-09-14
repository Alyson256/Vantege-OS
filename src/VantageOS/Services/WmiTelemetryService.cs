using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VantageOS.Models;

namespace VantageOS.Services
{
    public class WmiTelemetryService : ITelemetryService
    {
        private string _cpuModel = "Unknown CPU";
        private string _gpuModel = "Unknown GPU";
        
        // P/Invoke for fast RAM telemetry (O(1))
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private PerformanceCounter? _cpuCounter;

        public WmiTelemetryService()
        {
            InitializeStaticHardwareInfo();
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call always returns 0
            }
            catch { }
        }

        private void InitializeStaticHardwareInfo()
        {
            Task.Run(() =>
            {
                try
                {
                    using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                    foreach (var obj in cpuSearcher.Get())
                    {
                        _cpuModel = obj["Name"]?.ToString() ?? _cpuModel;
                        break;
                    }
                }
                catch { }

                try
                {
                    using var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                    foreach (var obj in gpuSearcher.Get())
                    {
                        _gpuModel = obj["Name"]?.ToString() ?? _gpuModel;
                        break;
                    }
                }
                catch { }
            });
        }

        public Task<TelemetrySnapshot> GetSnapshotAsync()
        {
            return Task.Run(() =>
            {
                var snapshot = new TelemetrySnapshot
                {
                    CpuModel = _cpuModel,
                    GpuModel = _gpuModel
                };

                // CPU
                if (_cpuCounter != null)
                {
                    snapshot.CpuUsage = Math.Round(_cpuCounter.NextValue(), 1);
                }

                // RAM
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    snapshot.RamTotalGB = Math.Round(memStatus.ullTotalPhys / 1024.0 / 1024.0 / 1024.0, 2);
                    snapshot.RamUsedGB = Math.Round((memStatus.ullTotalPhys - memStatus.ullAvailPhys) / 1024.0 / 1024.0 / 1024.0, 2);
                    snapshot.RamUsagePercentage = memStatus.dwMemoryLoad;
                }

                return snapshot;
            });
        }

        public async void StartMonitoring(Action<TelemetrySnapshot> onSnapshotUpdated, CancellationToken token)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(token))
            {
                if (token.IsCancellationRequested) break;
                var snapshot = await GetSnapshotAsync();
                onSnapshotUpdated(snapshot);
            }
        }
    }
}
