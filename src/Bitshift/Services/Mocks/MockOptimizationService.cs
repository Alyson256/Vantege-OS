using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockOptimizationService : IOptimizationService
    {
        private static readonly IReadOnlyDictionary<string, double> SampleSizes =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["win_cache"] = 482.6,
                ["temp_files"] = 316.2,
                ["update_cache"] = 1280.0,
                ["old_logs"] = 74.8,
                ["unused_files"] = 96.4
            };

        private readonly MockServiceOptions _options;

        public MockOptimizationService(MockServiceOptions options)
        {
            _options = options;
        }

        public async Task<bool> OptimizeRamAsync()
        {
            return (await OptimizeRamDetailedAsync()).Success;
        }

        public async Task<RamOptimizationResult> OptimizeRamDetailedAsync()
        {
            await _options.SimulateDelayAsync(700);
            _options.ThrowIfError();

            return new RamOptimizationResult
            {
                Success = !_options.IsEmpty,
                FreedMb = _options.IsEmpty ? 0 : 256.4,
                TrimmedProcesses = _options.IsEmpty ? 0 : 8,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            };
        }

        public async Task<Dictionary<string, double>> ScanCleanerCategoriesAsync(
            int unusedDaysThreshold = 30,
            CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(850, cancellationToken);
            _options.ThrowIfError();

            if (_options.IsEmpty) return SampleSizes.Keys.ToDictionary(key => key, _ => 0.0);
            return SampleSizes.ToDictionary(
                entry => entry.Key,
                entry => entry.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<CleanerExecutionResult> CleanSelectedCategoriesAsync(
            IReadOnlyCollection<string> categories,
            int unusedDaysThreshold = 30,
            CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(1200, cancellationToken);
            _options.ThrowIfError();

            var selected = new HashSet<string>(categories ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            if (_options.IsUacDenied && selected.Contains("update_cache"))
            {
                return new CleanerExecutionResult
                {
                    Success = false,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                };
            }

            var scannedSizes = _options.IsEmpty
                ? SampleSizes.Keys.ToDictionary(key => key, _ => 0.0)
                : SampleSizes.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value,
                    StringComparer.OrdinalIgnoreCase);
            double freedMb = selected.Where(scannedSizes.ContainsKey).Sum(category => scannedSizes[category]);

            return new CleanerExecutionResult
            {
                Success = true,
                FreedMb = Math.Round(freedMb, 1),
                DeletedFilesCount = freedMb == 0 ? 0 : 42,
                ScannedSizesMb = scannedSizes,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            };
        }

        public async Task<PowerProfileExecutionResult> ApplyPowerProfileAsync(
            string profileId,
            string? requestedGuid = null,
            CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(650, cancellationToken);
            _options.ThrowIfError();

            string normalizedProfile = string.IsNullOrWhiteSpace(profileId)
                ? "balanced"
                : profileId.Trim().ToLowerInvariant();

            bool knownProfile = normalizedProfile is "eco" or "balanced" or "performance" or "ultimate";
            bool success = !_options.IsEmpty && !_options.IsUacDenied && knownProfile;

            return new PowerProfileExecutionResult
            {
                Success = success,
                Profile = knownProfile ? normalizedProfile : "balanced",
                ActiveGuid = success ? $"mock-{normalizedProfile}" : string.Empty,
                Message = success
                    ? $"Plano '{normalizedProfile}' aplicado (simulado)."
                    : _options.IsUacDenied
                        ? "UAC negado (simulado)."
                        : _options.IsEmpty
                            ? "Nenhum plano de energia disponível no cenário vazio."
                            : "Perfil de energia desconhecido."
            };
        }

        public async Task<string> DetectActivePowerProfileAsync(CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(250, cancellationToken);
            _options.ThrowIfError();
            return _options.IsEmpty ? string.Empty : "balanced";
        }
    }
}
