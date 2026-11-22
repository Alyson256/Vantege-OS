using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bitshift.Models;
using Bitshift.Services;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ISystemSpecService _specService;
        private readonly ILocalizationService _localizationService;
        private readonly CancellationTokenSource _cts;
        private bool _disposed;

        // Rolling 60-second history buffers (populated strictly by real telemetry ticks)
        public ObservableCollection<double> CpuHistory { get; } = new();
        public ObservableCollection<double> RamHistory { get; } = new();
        public ObservableCollection<double> GpuHistory { get; } = new();

        // LiveCharts2 Series
        public ISeries[] CpuSeries { get; private set; }
        public ISeries[] RamSeries { get; private set; }
        public ISeries[] GpuSeries { get; private set; }

        public Axis[] HiddenXAxes { get; } = new Axis[] { new Axis { IsVisible = false } };
        public Axis[] StandardYAxes { get; } = new Axis[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = 100 } };

        [ObservableProperty]
        private string titleLabel = "Dashboard";

        [ObservableProperty]
        private string descriptionLabel = string.Empty;

        // ── CPU Specs & Telemetry (populated via Win32 / WMI) ──
        [ObservableProperty]
        private double cpuUsage;

        [ObservableProperty]
        private string cpuModel = "—";

        [ObservableProperty]
        private string cpuBaseClock = "—";

        [ObservableProperty]
        private string cpuBoostClock = "—";

        [ObservableProperty]
        private string cpuCores = "—";

        [ObservableProperty]
        private string cpuThreads = "—";

        [ObservableProperty]
        private string cpuCacheL3 = "—";

        [ObservableProperty]
        private string cpuSocket = "—";

        // ── RAM Specs & Telemetry (populated via GlobalMemoryStatusEx / WMI) ──
        [ObservableProperty]
        private double ramUsagePercentage;

        [ObservableProperty]
        private double ramUsedGB;

        [ObservableProperty]
        private double ramTotalGB;

        [ObservableProperty]
        private string ramSummary = "—";

        [ObservableProperty]
        private string ramType = "—";

        [ObservableProperty]
        private string ramSpeed = "—";

        [ObservableProperty]
        private string ramSlots = "—";

        [ObservableProperty]
        private string ramPartNumber = "—";

        // ── GPU Specs & Telemetry (populated via WDDM Counters / WMI / Registry) ──
        [ObservableProperty]
        private double gpuUsage;

        [ObservableProperty]
        private string gpuModel = "—";

        [ObservableProperty]
        private double gpuVramUsedGB;

        [ObservableProperty]
        private double gpuVramTotalGB;

        [ObservableProperty]
        private string gpuVram = "—";

        [ObservableProperty]
        private string gpuDriver = "—";

        [ObservableProperty]
        private string gpuDirectX = "—";

        [ObservableProperty]
        private string gpuResolution = "—";

        public DashboardViewModel(
            ITelemetryService telemetryService,
            ISystemSpecService specService,
            ILocalizationService localizationService)
        {
            _telemetryService = telemetryService;
            _specService = specService;
            _localizationService = localizationService;
            _cts = new CancellationTokenSource();

            CpuSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = CpuHistory,
                    Fill = new SolidColorPaint(SKColor.Parse("183b82f6")),
                    Stroke = new SolidColorPaint(SKColor.Parse("ff3b82f6")) { StrokeThickness = 1.8f },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            RamSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = RamHistory,
                    Fill = new SolidColorPaint(SKColor.Parse("1810b981")),
                    Stroke = new SolidColorPaint(SKColor.Parse("ff10b981")) { StrokeThickness = 1.8f },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            GpuSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = GpuHistory,
                    Fill = new SolidColorPaint(SKColor.Parse("18f59e0b")),
                    Stroke = new SolidColorPaint(SKColor.Parse("fff59e0b")) { StrokeThickness = 1.8f },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();
            _ = LoadStaticSpecsAsync();
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
        }

        private async Task LoadStaticSpecsAsync()
        {
            try
            {
                var specs = await _specService.GetSpecsAsync();
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null || _cts.IsCancellationRequested) return;

                dispatcher.BeginInvoke(() =>
                {
                    if (_cts.IsCancellationRequested) return;
                    var cpu = specs.Categories.FirstOrDefault(c => c.Name == "processor")?.Items;
                    if (cpu != null)
                    {
                        CpuModel = GetSpec(cpu, "lblModel");
                        var coresStr = GetSpec(cpu, "lblCores");
                        if (coresStr.Contains('/'))
                        {
                            var parts = coresStr.Split('/');
                            CpuCores = parts[0].Trim();
                            CpuThreads = parts[1].Trim();
                        }
                        CpuBaseClock = GetSpec(cpu, "lblBaseClock");
                        CpuBoostClock = GetSpec(cpu, "lblBoostClock");
                        CpuCacheL3 = GetSpec(cpu, "lblCacheL3");
                        CpuSocket = GetSpec(cpu, "lblSocket");
                    }

                    var ram = specs.Categories.FirstOrDefault(c => c.Name == "memory")?.Items;
                    if (ram != null)
                    {
                        string cap = GetSpec(ram, "lblTotalCapacity");
                        RamType = GetSpec(ram, "lblType");
                        RamSpeed = GetSpec(ram, "lblSpeed");
                        RamSlots = GetSpec(ram, "lblSlotsUsed");
                        RamPartNumber = GetSpec(ram, "lblPartNumber");
                        RamSummary = cap != "—" ? $"{cap} {RamType} {RamSpeed}".Trim() : "—";
                    }

                    var gpu = specs.Categories.FirstOrDefault(c => c.Name == "graphics")?.Items;
                    if (gpu != null)
                    {
                        GpuModel = GetSpec(gpu, "lblModel");
                        GpuVram = GetSpec(gpu, "lblVram");
                        GpuDriver = GetSpec(gpu, "lblDriver");
                        GpuDirectX = GetSpec(gpu, "lblDirectx");
                    }

                    var disp = specs.Categories.FirstOrDefault(c => c.Name == "displayNetwork")?.Items;
                    if (disp != null)
                    {
                        GpuResolution = GetSpec(disp, "lblResolution");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardViewModel] LoadStaticSpecsAsync fallback: {ex.Message}");
            }
        }

        private static string GetSpec(System.Collections.Generic.List<SpecItem> items, string key)
        {
            return items.FirstOrDefault(i => i.Label == key)?.Value ?? "—";
        }

        private void OnSnapshotUpdated(TelemetrySnapshot snapshot)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || _cts.IsCancellationRequested) return;

            dispatcher.BeginInvoke(() =>
            {
                if (_cts.IsCancellationRequested) return;

                CpuUsage = snapshot.CpuUsage;
                if (!string.IsNullOrWhiteSpace(snapshot.CpuModel)) CpuModel = snapshot.CpuModel;

                RamUsagePercentage = snapshot.RamUsagePercentage;
                if (snapshot.RamUsedGB > 0) RamUsedGB = snapshot.RamUsedGB;
                if (snapshot.RamTotalGB > 0) RamTotalGB = snapshot.RamTotalGB;

                GpuUsage = snapshot.GpuUsage;
                if (!string.IsNullOrWhiteSpace(snapshot.GpuModel)) GpuModel = snapshot.GpuModel;
                GpuVramUsedGB = snapshot.GpuVramUsedGB;
                if (snapshot.GpuVramTotalGB > 0) GpuVramTotalGB = snapshot.GpuVramTotalGB;

                if (CpuHistory.Count >= 60) CpuHistory.RemoveAt(0);
                CpuHistory.Add(snapshot.CpuUsage);

                if (RamHistory.Count >= 60) RamHistory.RemoveAt(0);
                RamHistory.Add(snapshot.RamUsagePercentage);

                if (GpuHistory.Count >= 60) GpuHistory.RemoveAt(0);
                GpuHistory.Add(snapshot.GpuUsage);
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _localizationService.LanguageChanged -= OnLanguageChanged;
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
