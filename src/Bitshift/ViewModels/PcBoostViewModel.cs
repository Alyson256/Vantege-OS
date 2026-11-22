using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bitshift.Models;
using Bitshift.Services;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class PcBoostViewModel : ObservableObject, IDisposable
    {
        private readonly IOptimizationService _optimizationService;
        private readonly ITelemetryService _telemetryService;
        private readonly ILocalizationService _localizationService;
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        [ObservableProperty]
        private string pageTitle = "PC Boost";

        [ObservableProperty]
        private string pageDesc = "Otimização rápida de energia, limpeza segura de arquivos temporários e liberação de memória RAM.";

        // Active boost power profile ("eco" | "balanced" | "performance")
        [ObservableProperty]
        private string activeBoostMode = "balanced";

        [ObservableProperty]
        private bool isApplyingPowerMode;

        [ObservableProperty]
        private string powerModeStatus = string.Empty;

        // Cleaner state
        [ObservableProperty]
        private bool cleanWinCache = true;

        [ObservableProperty]
        private bool cleanTempFiles = true;

        [ObservableProperty]
        private bool cleanUpdateCache;

        [ObservableProperty]
        private bool cleanOldLogs = true;

        [ObservableProperty]
        private bool cleanUnusedFiles;

        [ObservableProperty]
        private int unusedDaysThreshold = 30;

        [ObservableProperty]
        private string winCacheSizeText = "0.0 MB";

        [ObservableProperty]
        private string tempFilesSizeText = "0.0 MB";

        [ObservableProperty]
        private string updateCacheSizeText = "0.0 MB";

        [ObservableProperty]
        private string oldLogsSizeText = "0.0 MB";

        [ObservableProperty]
        private string unusedFilesSizeText = "0.0 MB";

        [ObservableProperty]
        private bool isScanningCleaner;

        [ObservableProperty]
        private bool isCleaningSystem;

        [ObservableProperty]
        private string cleanerStatus = string.Empty;

        // RAM optimizer state
        [ObservableProperty]
        private double ramUsagePercentage;

        [ObservableProperty]
        private double ramUsedGb;

        [ObservableProperty]
        private double ramTotalGb;

        [ObservableProperty]
        private bool isCleaningRam;

        [ObservableProperty]
        private string ramStatus = string.Empty;

        public PcBoostViewModel(
            IOptimizationService optimizationService,
            ITelemetryService telemetryService,
            ILocalizationService localizationService)
        {
            _optimizationService = optimizationService;
            _telemetryService = telemetryService;
            _localizationService = localizationService;

            _localizationService.LanguageChanged += UpdateLocalizedTexts;
            UpdateLocalizedTexts();

            _telemetryService.StartMonitoring(OnTelemetryUpdated, _cts.Token);
            _ = InitializeStateAsync();
        }

        private void UpdateLocalizedTexts()
        {
            bool isPt = _localizationService.CurrentLanguage == "pt";
            PageTitle = "PC Boost";
            PageDesc = isPt
                ? "Modos de energia do sistema, limpeza segura de temporários e otimização imediata de memória RAM."
                : "System power modes, safe temporary files cleanup, and instant RAM optimization.";
        }

        private async Task InitializeStateAsync()
        {
            try
            {
                string detected = await _optimizationService.DetectActivePowerProfileAsync(_cts.Token);
                if (!_cts.IsCancellationRequested)
                {
                    ActiveBoostMode = detected;
                }
            }
            catch
            {
                // Keep balanced fallback
            }

            await ScanCleanerAsync();
        }

        private void OnTelemetryUpdated(TelemetrySnapshot snapshot)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || _cts.IsCancellationRequested) return;

            dispatcher.BeginInvoke(() =>
            {
                if (_cts.IsCancellationRequested) return;
                RamUsagePercentage = snapshot.RamUsagePercentage;
                if (snapshot.RamUsedGB > 0) RamUsedGb = snapshot.RamUsedGB;
                if (snapshot.RamTotalGB > 0) RamTotalGb = snapshot.RamTotalGB;
            });
        }

        [RelayCommand]
        private async Task SelectPowerModeAsync(string profileId)
        {
            if (IsApplyingPowerMode || string.IsNullOrWhiteSpace(profileId)) return;

            IsApplyingPowerMode = true;
            try
            {
                var result = await _optimizationService.ApplyPowerProfileAsync(profileId, cancellationToken: _cts.Token);
                if (result.Success)
                {
                    ActiveBoostMode = result.Profile;
                    PowerModeStatus = _localizationService.CurrentLanguage == "pt"
                        ? $"Modo '{result.Profile}' ativado ({result.ActiveGuid})."
                        : $"Profile '{result.Profile}' activated ({result.ActiveGuid}).";
                }
                else
                {
                    PowerModeStatus = result.Message;
                }
            }
            catch (Exception ex)
            {
                PowerModeStatus = ex.Message;
            }
            finally
            {
                IsApplyingPowerMode = false;
            }
        }

        [RelayCommand]
        private async Task ScanCleanerAsync()
        {
            if (IsScanningCleaner) return;
            IsScanningCleaner = true;
            try
            {
                var sizes = await _optimizationService.ScanCleanerCategoriesAsync(UnusedDaysThreshold, _cts.Token);
                ApplyScannedSizes(sizes);
            }
            catch
            {
                // Keep previous scan totals if interrupted
            }
            finally
            {
                IsScanningCleaner = false;
            }
        }

        [RelayCommand]
        private async Task CleanSystemAsync()
        {
            if (IsCleaningSystem) return;

            var categories = new List<string>();
            if (CleanWinCache) categories.Add("win_cache");
            if (CleanTempFiles) categories.Add("temp_files");
            if (CleanUpdateCache) categories.Add("update_cache");
            if (CleanOldLogs) categories.Add("old_logs");
            if (CleanUnusedFiles) categories.Add("unused_files");

            if (categories.Count == 0)
            {
                CleanerStatus = _localizationService.CurrentLanguage == "pt"
                    ? "Selecione ao menos uma categoria para limpar."
                    : "Select at least one category to clean.";
                return;
            }

            IsCleaningSystem = true;
            try
            {
                var res = await _optimizationService.CleanSelectedCategoriesAsync(categories, UnusedDaysThreshold, _cts.Token);
                ApplyScannedSizes(res.ScannedSizesMb);
                CleanerStatus = _localizationService.CurrentLanguage == "pt"
                    ? $"Limpeza concluída: {res.FreedMb:F1} MB liberados ({res.DeletedFilesCount} arquivo(s))."
                    : $"Cleanup finished: {res.FreedMb:F1} MB freed ({res.DeletedFilesCount} file(s)).";
            }
            catch (Exception ex)
            {
                CleanerStatus = ex.Message;
            }
            finally
            {
                IsCleaningSystem = false;
            }
        }

        [RelayCommand]
        private async Task CleanRamAsync()
        {
            if (IsCleaningRam) return;
            IsCleaningRam = true;
            try
            {
                var res = await _optimizationService.OptimizeRamDetailedAsync();
                RamStatus = res.Success
                    ? (_localizationService.CurrentLanguage == "pt"
                        ? $"RAM otimizada ({res.Timestamp}): {res.FreedMb:F1} MB liberados em {res.TrimmedProcesses} processos."
                        : $"RAM optimized ({res.Timestamp}): {res.FreedMb:F1} MB freed across {res.TrimmedProcesses} processes.")
                    : _localizationService.Get("trayRamErrorMsg");
            }
            catch (Exception ex)
            {
                RamStatus = ex.Message;
            }
            finally
            {
                IsCleaningRam = false;
            }
        }

        private void ApplyScannedSizes(Dictionary<string, double> sizes)
        {
            WinCacheSizeText = $"{sizes.GetValueOrDefault("win_cache"):F1} MB";
            TempFilesSizeText = $"{sizes.GetValueOrDefault("temp_files"):F1} MB";
            UpdateCacheSizeText = $"{sizes.GetValueOrDefault("update_cache"):F1} MB";
            OldLogsSizeText = $"{sizes.GetValueOrDefault("old_logs"):F1} MB";
            UnusedFilesSizeText = $"{sizes.GetValueOrDefault("unused_files"):F1} MB";
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _localizationService.LanguageChanged -= UpdateLocalizedTexts;
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
