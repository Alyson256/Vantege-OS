using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public class OptimizationService : IOptimizationService
    {
        private const string EcoPlanGuid = "a1841308-3541-4fab-bc81-f71556f20b4a";
        private const string BalancedPlanGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string HighPerfPlanGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
        private const string UltimatePerfPlanGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61";

        private static readonly HashSet<string> SafeUnusedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".tmp", ".temp", ".log", ".bak", ".old", ".dmp", ".part", ".crdownload"
        };

        private static readonly Regex GuidRegex = new(
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            RegexOptions.Compiled);

        private readonly IBatchExecutionService _batchService;

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public OptimizationService(IBatchExecutionService batchService)
        {
            _batchService = batchService;
        }

        public async Task<bool> OptimizeRamAsync()
        {
            var result = await OptimizeRamDetailedAsync();
            return result.Success;
        }

        public async Task<RamOptimizationResult> OptimizeRamDetailedAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    ulong availBefore = ReadAvailablePhysicalBytes();
                    int trimmed = 0;

                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        using (process)
                        {
                            try
                            {
                                if (EmptyWorkingSet(process.Handle) != 0)
                                {
                                    trimmed++;
                                }
                            }
                            catch
                            {
                                // Protected system processes deny handle access by design
                            }
                        }
                    }

                    ulong availAfter = ReadAvailablePhysicalBytes();
                    double freedMb = availAfter > availBefore
                        ? Math.Round((availAfter - availBefore) / 1024.0 / 1024.0, 1)
                        : 0.0;

                    return new RamOptimizationResult
                    {
                        Success = true,
                        FreedMb = freedMb,
                        TrimmedProcesses = trimmed,
                        Timestamp = DateTime.Now.ToString("HH:mm:ss")
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OptimizationService] OptimizeRamDetailedAsync failed: {ex.Message}");
                    return new RamOptimizationResult
                    {
                        Success = false,
                        Timestamp = DateTime.Now.ToString("HH:mm:ss")
                    };
                }
            });
        }

        public async Task<Dictionary<string, double>> ScanCleanerCategoriesAsync(
            int unusedDaysThreshold = 30,
            CancellationToken cancellationToken = default)
        {
            int clampedDays = Math.Clamp(unusedDaysThreshold, 7, 120);

            return await Task.Run(() =>
            {
                var results = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["win_cache"] = 0,
                    ["temp_files"] = 0,
                    ["update_cache"] = 0,
                    ["old_logs"] = 0,
                    ["unused_files"] = 0
                };

                try
                {
                    results["win_cache"] = BytesToMb(CalculateTargetBytes("win_cache", clampedDays, cancellationToken));
                    results["temp_files"] = BytesToMb(CalculateTargetBytes("temp_files", clampedDays, cancellationToken));
                    results["update_cache"] = BytesToMb(CalculateTargetBytes("update_cache", clampedDays, cancellationToken));
                    results["old_logs"] = BytesToMb(CalculateTargetBytes("old_logs", clampedDays, cancellationToken));
                    results["unused_files"] = BytesToMb(CalculateTargetBytes("unused_files", clampedDays, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    // Cancelled cleanly
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OptimizationService] ScanCleanerCategoriesAsync error: {ex.Message}");
                }

                return results;
            }, cancellationToken);
        }

        public async Task<CleanerExecutionResult> CleanSelectedCategoriesAsync(
            IReadOnlyCollection<string> categories,
            int unusedDaysThreshold = 30,
            CancellationToken cancellationToken = default)
        {
            int clampedDays = Math.Clamp(unusedDaysThreshold, 7, 120);
            var selected = new HashSet<string>(categories ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            long freedBytes = 0;
            int deletedCount = 0;

            if (selected.Contains("update_cache"))
            {
                await _batchService.ExecuteCommandAsync("net", "stop wuauserv", cancellationToken: cancellationToken);
                await _batchService.ExecuteCommandAsync("net", "stop bits", cancellationToken: cancellationToken);
            }

            try
            {
                await Task.Run(() =>
                {
                    foreach (string category in selected)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        var (bytes, files) = CleanCategoryInternal(category, clampedDays, cancellationToken);
                        freedBytes += bytes;
                        deletedCount += files;
                    }
                }, cancellationToken);
            }
            finally
            {
                if (selected.Contains("update_cache"))
                {
                    await _batchService.ExecuteCommandAsync("net", "start wuauserv", cancellationToken: CancellationToken.None);
                    await _batchService.ExecuteCommandAsync("net", "start bits", cancellationToken: CancellationToken.None);
                }
            }

            var updatedScan = await ScanCleanerCategoriesAsync(clampedDays, cancellationToken);

            return new CleanerExecutionResult
            {
                Success = true,
                FreedMb = BytesToMb(freedBytes),
                DeletedFilesCount = deletedCount,
                ScannedSizesMb = updatedScan,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            };
        }

        public async Task<PowerProfileExecutionResult> ApplyPowerProfileAsync(
            string profileId,
            string? requestedGuid = null,
            CancellationToken cancellationToken = default)
        {
            string normalizedProfile = string.IsNullOrWhiteSpace(profileId)
                ? "balanced"
                : profileId.Trim().ToLowerInvariant();

            string targetGuid = normalizedProfile switch
            {
                "eco" => EcoPlanGuid,
                "balanced" => BalancedPlanGuid,
                "performance" => HighPerfPlanGuid,
                _ => BalancedPlanGuid
            };

            if (!string.IsNullOrWhiteSpace(requestedGuid) && Guid.TryParse(requestedGuid, out Guid parsed))
            {
                targetGuid = parsed.ToString("D");
            }

            var res = await _batchService.ExecuteCommandAsync("powercfg", $"/setactive {targetGuid}", cancellationToken: cancellationToken);
            if (!res.Success)
            {
                await _batchService.ExecuteCommandAsync("powercfg", $"-duplicatescheme {targetGuid}", cancellationToken: cancellationToken);
                res = await _batchService.ExecuteCommandAsync("powercfg", $"/setactive {targetGuid}", cancellationToken: cancellationToken);

                if (!res.Success && normalizedProfile == "performance")
                {
                    await _batchService.ExecuteCommandAsync("powercfg", $"-duplicatescheme {UltimatePerfPlanGuid}", cancellationToken: cancellationToken);
                    var ultRes = await _batchService.ExecuteCommandAsync("powercfg", $"/setactive {UltimatePerfPlanGuid}", cancellationToken: cancellationToken);
                    if (ultRes.Success)
                    {
                        targetGuid = UltimatePerfPlanGuid;
                        res = ultRes;
                    }
                }
            }

            return new PowerProfileExecutionResult
            {
                Success = res.Success,
                Profile = normalizedProfile,
                ActiveGuid = targetGuid,
                Message = res.Success ? $"Plano aplicado ({targetGuid})" : (string.IsNullOrWhiteSpace(res.Error) ? res.Output : res.Error)
            };
        }

        public async Task<string> DetectActivePowerProfileAsync(CancellationToken cancellationToken = default)
        {
            var res = await _batchService.ExecuteCommandAsync("powercfg", "/getactivescheme", cancellationToken: cancellationToken);
            if (!res.Success || string.IsNullOrWhiteSpace(res.Output))
            {
                return "balanced";
            }

            Match match = GuidRegex.Match(res.Output);
            if (!match.Success) return "balanced";

            string guid = match.Value.ToLowerInvariant();
            return guid switch
            {
                EcoPlanGuid => "eco",
                HighPerfPlanGuid or UltimatePerfPlanGuid => "performance",
                _ => "balanced"
            };
        }

        private static IEnumerable<(string DirectoryPath, Func<FileInfo, bool>? Predicate, bool Recursive)> GetCategoryTargets(
            string category,
            int unusedDaysThreshold)
        {
            string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DateTime cutoffUtc = DateTime.UtcNow.AddDays(-unusedDaysThreshold);

            return category.ToLowerInvariant() switch
            {
                "win_cache" => new (string, Func<FileInfo, bool>?, bool)[]
                {
                    (Path.Combine(windowsDir, "Prefetch"), null, false),
                    (Path.Combine(localAppData, "D3DSCache"), null, true),
                    (Path.Combine(localAppData, "FontCache"), null, true),
                    (Path.Combine(localAppData, "Microsoft", "Windows", "Explorer"),
                        f => f.Name.StartsWith("thumbcache_", StringComparison.OrdinalIgnoreCase) ||
                             f.Name.StartsWith("iconcache_", StringComparison.OrdinalIgnoreCase),
                        false)
                },
                "temp_files" => new (string, Func<FileInfo, bool>?, bool)[]
                {
                    (Path.GetTempPath(), null, true),
                    (Path.Combine(windowsDir, "Temp"), null, true)
                },
                "update_cache" => new (string, Func<FileInfo, bool>?, bool)[]
                {
                    (Path.Combine(windowsDir, "SoftwareDistribution", "Download"), null, true)
                },
                "old_logs" => new (string, Func<FileInfo, bool>?, bool)[]
                {
                    (Path.Combine(windowsDir, "Logs"), f => f.Extension.Equals(".log", StringComparison.OrdinalIgnoreCase), true),
                    (Path.Combine(localAppData, "CrashDumps"), null, true),
                    (Path.Combine(programData, "Microsoft", "Windows", "WER"), null, true)
                },
                "unused_files" => new (string, Func<FileInfo, bool>?, bool)[]
                {
                    (Path.Combine(userProfile, "Downloads"),
                        f => SafeUnusedExtensions.Contains(f.Extension) &&
                             f.LastAccessTimeUtc < cutoffUtc &&
                             f.LastWriteTimeUtc < cutoffUtc,
                        false),
                    (Path.Combine(localAppData, "Temp"),
                        f => f.LastAccessTimeUtc < cutoffUtc && f.LastWriteTimeUtc < cutoffUtc,
                        true)
                },
                _ => Array.Empty<(string, Func<FileInfo, bool>?, bool)>()
            };
        }

        private static long CalculateTargetBytes(string category, int unusedDaysThreshold, CancellationToken ct)
        {
            long total = 0;
            foreach (var (dirPath, predicate, recursive) in GetCategoryTargets(category, unusedDaysThreshold))
            {
                if (ct.IsCancellationRequested || string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
                {
                    continue;
                }

                total += EnumerateFilesSafe(dirPath, recursive, ct)
                    .Where(f => predicate == null || predicate(f))
                    .Sum(f =>
                    {
                        try { return f.Length; } catch { return 0L; }
                    });
            }
            return total;
        }

        private static (long FreedBytes, int DeletedFiles) CleanCategoryInternal(
            string category,
            int unusedDaysThreshold,
            CancellationToken ct)
        {
            long freedBytes = 0;
            int deletedCount = 0;

            foreach (var (dirPath, predicate, recursive) in GetCategoryTargets(category, unusedDaysThreshold))
            {
                if (ct.IsCancellationRequested || string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
                {
                    continue;
                }

                foreach (var file in EnumerateFilesSafe(dirPath, recursive, ct))
                {
                    if (ct.IsCancellationRequested) break;
                    if (predicate != null && !predicate(file)) continue;

                    try
                    {
                        long len = file.Length;
                        file.IsReadOnly = false;
                        file.Delete();
                        freedBytes += len;
                        deletedCount++;
                    }
                    catch
                    {
                        // Locked by active Windows process — skip safely
                    }
                }

                if (recursive && predicate == null)
                {
                    TryDeleteEmptySubdirectories(dirPath);
                }
            }

            return (freedBytes, deletedCount);
        }

        private static IEnumerable<FileInfo> EnumerateFilesSafe(string rootDir, bool recursive, CancellationToken ct)
        {
            var stack = new Stack<string>();
            stack.Push(rootDir);

            while (stack.Count > 0)
            {
                if (ct.IsCancellationRequested) yield break;
                string current = stack.Pop();

                FileInfo[] files;
                try
                {
                    files = new DirectoryInfo(current).GetFiles();
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    yield return file;
                }

                if (!recursive) continue;

                try
                {
                    foreach (var subDir in new DirectoryInfo(current).GetDirectories())
                    {
                        if ((subDir.Attributes & FileAttributes.ReparsePoint) == 0)
                        {
                            stack.Push(subDir.FullName);
                        }
                    }
                }
                catch
                {
                    // Access denied on protected subdirectory
                }
            }
        }

        private static void TryDeleteEmptySubdirectories(string rootDir)
        {
            try
            {
                foreach (var sub in Directory.GetDirectories(rootDir))
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(sub).Any())
                        {
                            Directory.Delete(sub, false);
                        }
                    }
                    catch
                    {
                        // Directory non-empty or locked
                    }
                }
            }
            catch
            {
                // Ignore root enumeration restrictions
            }
        }

        private static double BytesToMb(long bytes)
        {
            return Math.Round(bytes / 1024.0 / 1024.0, 1);
        }

        private static ulong ReadAvailablePhysicalBytes()
        {
            var status = new WmiTelemetryService.MEMORYSTATUSEX
            {
                dwLength = (uint)Marshal.SizeOf(typeof(WmiTelemetryService.MEMORYSTATUSEX))
            };
            return WmiTelemetryService.GlobalMemoryStatusEx(ref status) ? status.ullAvailPhys : 0;
        }
    }
}
