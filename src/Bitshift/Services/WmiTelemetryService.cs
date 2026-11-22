using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    /// <summary>
    /// Telemetria Direta de Baixo Nível (O(1)) sem Overhead e 100% Anti-Cheat Safe:
    /// - CPU: PerformanceCounter nativo com fallback para GetSystemTimes (Kernel32 P/Invoke).
    /// - RAM: GlobalMemoryStatusEx via Win32 P/Invoke (O(1), zero alocação).
    /// - GPU/Disco: PerformanceCounterCategory nativo WDDM / PhysicalDisk + Registry VRAM 64-bit.
    /// - WMI: Executado estritamente 1 única vez na inicialização para metadados estáticos.
    /// - Zero drivers de Kernel (Ring-0 / WinRing0.sys).
    /// </summary>
    public class WmiTelemetryService : ITelemetryService, IDisposable
    {
        private bool _disposed;
        private string _cpuModel = string.Empty;
        private string _gpuModel = string.Empty;
        private double _gpuVramTotalGb;

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

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

        private long _prevIdleTime;
        private long _prevKernelTime;
        private long _prevUserTime;

        private readonly object _countersLock = new();
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _diskCounter;
        private readonly List<PerformanceCounter> _gpuEngineCounters = new();
        private PerformanceCounter? _gpuDedicatedMemoryCounter;

        public WmiTelemetryService()
        {
            try
            {
                GetSystemTimes(out _prevIdleTime, out _prevKernelTime, out _prevUserTime);
            }
            catch
            {
                // Ignore if running outside standard Win32 host
            }

            InitializeStaticHardwareInfoOnce();
            _ = Task.Run(InitializeCounters);
        }

        private void InitializeCounters()
        {
            if (_disposed) return;

            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
            }
            catch
            {
                _cpuCounter = null;
            }

            try
            {
                _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                _diskCounter.NextValue();
            }
            catch
            {
                _diskCounter = null;
            }

            InitializeGpuCountersSafe();
        }

        private void InitializeGpuCountersSafe()
        {
            var discovered = new List<PerformanceCounter>();
            try
            {
                if (PerformanceCounterCategory.Exists("GPU Engine"))
                {
                    var cat = new PerformanceCounterCategory("GPU Engine");
                    foreach (var instance in cat.GetInstanceNames())
                    {
                        if (!instance.Contains("engtype_3D", StringComparison.OrdinalIgnoreCase)) continue;
                        PerformanceCounter? counter = null;
                        try
                        {
                            counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance);
                            counter.NextValue();
                            discovered.Add(counter);
                        }
                        catch
                        {
                            counter?.Dispose();
                        }
                    }
                }
            }
            catch
            {
                foreach (var c in discovered) c.Dispose();
                discovered.Clear();
            }

            lock (_countersLock)
            {
                if (_disposed)
                {
                    foreach (var c in discovered) c.Dispose();
                }
                else
                {
                    _gpuEngineCounters.Clear();
                    _gpuEngineCounters.AddRange(discovered);
                }
            }

            try
            {
                if (!_disposed && PerformanceCounterCategory.Exists("GPU Adapter Memory"))
                {
                    var memCat = new PerformanceCounterCategory("GPU Adapter Memory");
                    var firstInstance = memCat.GetInstanceNames().FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstInstance))
                    {
                        var memCounter = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage", firstInstance);
                        memCounter.NextValue();
                        _gpuDedicatedMemoryCounter = memCounter;
                    }
                }
            }
            catch
            {
                _gpuDedicatedMemoryCounter = null;
            }
        }

        private void InitializeStaticHardwareInfoOnce()
        {
            Task.Run(() =>
            {
                try
                {
                    using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                    foreach (var obj in cpuSearcher.Get())
                    {
                        _cpuModel = obj["Name"]?.ToString()?.Trim() ?? string.Empty;
                        break;
                    }
                }
                catch { }

                try
                {
                    using var gpuSearcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                    var gpuCandidates = new List<(string Name, ulong VramBytes, bool IsSoftware)>();
                    foreach (ManagementObject obj in gpuSearcher.Get())
                    {
                        using (obj)
                        {
                            string name = obj["Name"]?.ToString()?.Trim() ?? string.Empty;
                            ulong vramBytes = Win32HardwareHelpers.ReadGpuMemoryBytesFromRegistry(name);
                            if (vramBytes == 0 && obj["AdapterRAM"] != null)
                            {
                                vramBytes = Convert.ToUInt64(obj["AdapterRAM"]);
                            }
                            bool isSoftware = Win32HardwareHelpers.IsSoftwareGpuAdapter(name);
                            gpuCandidates.Add((name, vramBytes, isSoftware));
                        }
                    }

                    if (gpuCandidates.Count > 0)
                    {
                        var best = gpuCandidates
                            .OrderBy(c => c.IsSoftware ? 1 : 0)
                            .ThenByDescending(c => c.VramBytes)
                            .First();
                        _gpuModel = best.Name;
                        if (best.VramBytes > 0)
                        {
                            _gpuVramTotalGb = Math.Round(best.VramBytes / 1024.0 / 1024.0 / 1024.0, 2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WmiTelemetryService] GPU init error: {ex.Message}");
                }
            });
        }

        public Task<TelemetrySnapshot> GetSnapshotAsync()
        {
            return Task.Run(() =>
            {
                var snapshot = new TelemetrySnapshot
                {
                    CpuModel = _cpuModel,
                    GpuModel = _gpuModel,
                    GpuVramTotalGB = _gpuVramTotalGb
                };

                // 1. CPU via PerformanceCounter or Kernel32 GetSystemTimes
                snapshot.CpuUsage = GetCpuUsageSafe();

                // 2. RAM via Win32 GlobalMemoryStatusEx
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    snapshot.RamTotalGB = Math.Round(memStatus.ullTotalPhys / 1024.0 / 1024.0 / 1024.0, 2);
                    snapshot.RamUsedGB = Math.Round((memStatus.ullTotalPhys - memStatus.ullAvailPhys) / 1024.0 / 1024.0 / 1024.0, 2);
                    snapshot.RamUsagePercentage = memStatus.dwMemoryLoad;
                }

                // 3. GPU via Windows WDDM Performance Counters (real data only, no estimation)
                snapshot.GpuUsage = GetGpuUsageSafe();
                snapshot.GpuVramUsedGB = GetGpuVramUsedSafe();

                // 4. Physical Disk activity
                snapshot.DiskActiveTime = GetDiskActiveTimeSafe();

                return snapshot;
            });
        }

        private double GetCpuUsageSafe()
        {
            if (_cpuCounter != null)
            {
                try
                {
                    return Math.Round(_cpuCounter.NextValue(), 1);
                }
                catch { }
            }

            if (GetSystemTimes(out long idleTime, out long kernelTime, out long userTime))
            {
                long dIdle = idleTime - _prevIdleTime;
                long dKernel = kernelTime - _prevKernelTime;
                long dUser = userTime - _prevUserTime;

                _prevIdleTime = idleTime;
                _prevKernelTime = kernelTime;
                _prevUserTime = userTime;

                long total = dKernel + dUser;
                if (total > 0)
                {
                    double cpu = ((total - dIdle) * 100.0) / total;
                    return Math.Max(0.0, Math.Min(100.0, Math.Round(cpu, 1)));
                }
            }

            return 0.0;
        }

        private double GetGpuUsageSafe()
        {
            PerformanceCounter[] snapshot;
            lock (_countersLock)
            {
                if (_gpuEngineCounters.Count == 0) return 0.0;
                snapshot = _gpuEngineCounters.ToArray();
            }

            double sum = 0;
            foreach (var counter in snapshot)
            {
                try
                {
                    sum += counter.NextValue();
                }
                catch
                {
                    // Skip exited process GPU engine instance without losing remaining GPU telemetry
                }
            }
            return Math.Max(0.0, Math.Min(100.0, Math.Round(sum, 1)));
        }

        private double GetGpuVramUsedSafe()
        {
            if (_gpuDedicatedMemoryCounter == null) return 0.0;
            try
            {
                float bytes = _gpuDedicatedMemoryCounter.NextValue();
                return Math.Max(0.0, Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 2));
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetDiskActiveTimeSafe()
        {
            if (_diskCounter == null) return 0.0;
            try
            {
                return Math.Max(0.0, Math.Min(100.0, Math.Round(_diskCounter.NextValue(), 1)));
            }
            catch
            {
                return 0.0;
            }
        }

        public async void StartMonitoring(Action<TelemetrySnapshot> onSnapshotUpdated, CancellationToken token)
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (!_disposed && await timer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested || _disposed) break;
                    var snapshot = await GetSnapshotAsync();
                    onSnapshotUpdated(snapshot);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WmiTelemetryService] StartMonitoring error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cpuCounter?.Dispose();
            _cpuCounter = null;

            _diskCounter?.Dispose();
            _diskCounter = null;

            _gpuDedicatedMemoryCounter?.Dispose();
            _gpuDedicatedMemoryCounter = null;

            lock (_countersLock)
            {
                foreach (var counter in _gpuEngineCounters)
                {
                    counter.Dispose();
                }
                _gpuEngineCounters.Clear();
            }

            GC.SuppressFinalize(this);
        }
    }
}
