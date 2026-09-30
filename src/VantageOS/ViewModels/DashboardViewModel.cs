using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VantageOS.Models;
using VantageOS.Services;

namespace VantageOS.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILocalizationService _localizationService;
        private readonly IOptimizationService _optimizationService;
        private readonly CancellationTokenSource _cts;

        [ObservableProperty]
        private string titleLabel = "Dashboard";

        [ObservableProperty]
        private string descriptionLabel = "Visão geral do sistema e telemetria em tempo real";

        [ObservableProperty]
        private string ramLoadLabel = "RAM Load";

        [ObservableProperty]
        private double cpuUsage;

        [ObservableProperty]
        private string cpuModel = "Loading...";

        [ObservableProperty]
        private string gpuModel = "Loading...";

        [ObservableProperty]
        private double ramUsagePercentage;

        [ObservableProperty]
        private double ramUsedGB;

        [ObservableProperty]
        private double ramTotalGB;

        [ObservableProperty]
        private double dpcLatency;

        [ObservableProperty]
        private double isrLatency;

        [ObservableProperty]
        private bool isOptimizingRam;

        [ObservableProperty]
        private bool isRamOptimized;

        [ObservableProperty]
        private bool isCleaningJunk;

        [ObservableProperty]
        private bool isJunkCleaned;

        [ObservableProperty]
        private string ramOptimizerTitle = "Otimização de RAM";

        [ObservableProperty]
        private string ramOptimizerDesc = "Libera memória em cache e processos suspensos";

        [ObservableProperty]
        private string junkCleanerTitle = "Limpeza de Disco";

        [ObservableProperty]
        private string junkCleanerDesc = "Arquivos temporários e cache do sistema";

        public DashboardViewModel(ITelemetryService telemetryService, ILocalizationService localizationService, IOptimizationService optimizationService)
        {
            _telemetryService = telemetryService;
            _localizationService = localizationService;
            _optimizationService = optimizationService;
            _cts = new CancellationTokenSource();
            
            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();

            _telemetryService.StartMonitoring(OnSnapshotUpdated, _cts.Token);
        }

        private void OnLanguageChanged()
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            TitleLabel = _localizationService.Get("dashboard");
            DescriptionLabel = _localizationService.Get("dashDesc");
            RamLoadLabel = _localizationService.Get("ramUsage") == "ramUsage" ? "RAM Load" : _localizationService.Get("ramUsage");
            RamOptimizerTitle = _localizationService.Get("ramTitle");
            RamOptimizerDesc = _localizationService.Get("ramDesc");
            JunkCleanerTitle = _localizationService.Get("junkFilesTitle");
            JunkCleanerDesc = _localizationService.Get("junkFilesDesc");
        }

        [RelayCommand]
        private async Task OptimizeRamAsync()
        {
            if (IsOptimizingRam || IsRamOptimized) return;
            IsOptimizingRam = true;
            
            await _optimizationService.OptimizeRamAsync();
            
            IsOptimizingRam = false;
            IsRamOptimized = true;
            
            await Task.Delay(3000);
            IsRamOptimized = false;
        }

        [RelayCommand]
        private async Task CleanJunkAsync()
        {
            if (IsCleaningJunk || IsJunkCleaned) return;
            IsCleaningJunk = true;
            
            await _optimizationService.CleanJunkFilesAsync();
            
            IsCleaningJunk = false;
            IsJunkCleaned = true;
            
            await Task.Delay(3000);
            IsJunkCleaned = false;
        }

        private void OnSnapshotUpdated(TelemetrySnapshot snapshot)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage = snapshot.CpuUsage;
                CpuModel = snapshot.CpuModel;
                GpuModel = snapshot.GpuModel;
                RamUsagePercentage = snapshot.RamUsagePercentage;
                RamUsedGB = snapshot.RamUsedGB;
                RamTotalGB = snapshot.RamTotalGB;
                DpcLatency = snapshot.DpcLatency;
                IsrLatency = snapshot.IsrLatency;
            });
        }
        
        ~DashboardViewModel()
        {
            _cts.Cancel();
        }
    }
}
