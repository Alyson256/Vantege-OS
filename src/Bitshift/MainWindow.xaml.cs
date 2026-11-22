using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Bitshift.Services;
using Bitshift.Services.Abstractions;
using Bitshift.ViewModels;
using Wpf.Ui.Controls;

namespace Bitshift
{
    public partial class MainWindow : FluentWindow
    {
        private readonly ISettingsService _settingsService;
        private readonly ITrayIconService _trayIconService;
        private readonly IOptimizationService _optimizationService;
        private readonly ILocalizationService _localizationService;

        public MainWindow(
            MainViewModel viewModel,
            ISettingsService settingsService,
            ITrayIconService trayIconService,
            IOptimizationService optimizationService,
            ILocalizationService localizationService)
        {
            InitializeComponent();
            DataContext = viewModel;

            _settingsService = settingsService;
            _trayIconService = trayIconService;
            _optimizationService = optimizationService;
            _localizationService = localizationService;

            try
            {
                _trayIconService.Initialize(
                    onOpenRequested: RestoreFromTray,
                    onCleanRamRequested: CleanRamFromTrayAsync,
                    onExitRequested: ExitFromTray);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Tray initialization skipped: {ex.Message}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) &&
                    WindowBackdrop.IsSupported(WindowBackdropType.Mica))
                {
                    WindowBackdropType = WindowBackdropType.Mica;
                    WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
                }
                else
                {
                    WindowBackdropType = WindowBackdropType.None;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Backdrop fallback applied: {ex.Message}");
                WindowBackdropType = WindowBackdropType.None;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized && _settingsService.Current.MinimizeToTray)
            {
                ShowInTaskbar = false;
                Hide();
                _trayIconService.SetVisible(true);

                if (_settingsService.Current.ShowTrayNotificationOnMinimize)
                {
                    _trayIconService.ShowNotification(
                        "BITSHIFT",
                        _localizationService.Get("trayMinimizedMsg"));
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }
            _trayIconService.SetVisible(false);
            _trayIconService.Dispose();
            base.OnClosed(e);
        }

        private void RestoreFromTray()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = WindowState.Normal;
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
            _trayIconService.SetVisible(false);
        }

        private async Task CleanRamFromTrayAsync()
        {
            bool ok = await _optimizationService.OptimizeRamAsync();
            string message = ok
                ? _localizationService.Get("trayRamCleanedMsg")
                : _localizationService.Get("trayRamErrorMsg");
            _trayIconService.ShowNotification("BITSHIFT", message, isError: !ok);
        }

        private void ExitFromTray()
        {
            _trayIconService.SetVisible(false);
            _trayIconService.Dispose();
            Application.Current?.Shutdown();
        }
    }
}