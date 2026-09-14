using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Windows;
using VantageOS.Models;
using VantageOS.Services;

namespace VantageOS.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITelemetryService _telemetryService;
        private readonly CancellationTokenSource _cts;

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

        public DashboardViewModel(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
            _cts = new CancellationTokenSource();
            
            _telemetryService.StartMonitoring(OnSnapshotUpdated, _cts.Token);
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
