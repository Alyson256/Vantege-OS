using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Bitshift.ViewModels;
using Bitshift.Services;
using Bitshift.Services.Abstractions;
using Bitshift.Services.Mocks;

namespace Bitshift
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            bool useMockServices = ResolveUseMockServices();

            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IWindowsThemeService, WindowsThemeService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ITrayIconService, TrayIconService>();
            services.AddSingleton<IDialogService, WpfDialogService>();

            if (useMockServices)
            {
                services.AddSingleton<MockServiceOptions>(_ => MockServiceOptions.FromEnvironment());
                services.AddSingleton<IBatchExecutionService, MockBatchExecutionService>();
                services.AddSingleton<ITelemetryService, MockTelemetryService>();
                services.AddSingleton<ISystemSnapshotService, MockSystemSnapshotService>();
                services.AddSingleton<IOptimizationService, MockOptimizationService>();
                services.AddSingleton<ISystemSpecService, MockSystemSpecService>();
                services.AddSingleton<ITweakService, MockTweakService>();
                services.AddSingleton<IUpdateCheckService, MockUpdateCheckService>();
                services.AddSingleton<ILauncher, MockLauncher>();
                services.AddSingleton<IClipboardService, MockClipboardService>();
            }
            else
            {
                services.AddSingleton<IBatchExecutionService, BatchExecutionService>();
                services.AddSingleton<ITelemetryService, WmiTelemetryService>();
                services.AddSingleton<ISystemSnapshotService, SystemSnapshotService>();
                services.AddSingleton<IOptimizationService, OptimizationService>();
                services.AddSingleton<ISystemSpecService, SystemSpecService>();
                services.AddSingleton<ITweakService, TweakService>();
                services.AddSingleton<IUpdateCheckService, UpdateCheckService>();
                services.AddSingleton<ILauncher, ShellLauncher>();
                services.AddSingleton<IClipboardService, ClipboardService>();
            }

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<PcBoostViewModel>();
            services.AddTransient<SystemSpecsViewModel>();
            services.AddTransient<CustomTweaksViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AboutViewModel>();

            // Views
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        private static bool ResolveUseMockServices()
        {
            string? configuredValue = Environment.GetEnvironmentVariable("BITSHIFT_USE_MOCK_SERVICES");
            if (!string.IsNullOrWhiteSpace(configuredValue))
            {
                if (!bool.TryParse(configuredValue, out bool configured))
                {
                    throw new InvalidOperationException(
                        "BITSHIFT_USE_MOCK_SERVICES must be true or false.");
                }

                return configured;
            }

#if DEBUG
            return true;
#else
            return false;
#endif
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Apply saved user language before building MainWindow
                var settingsService = Services.GetRequiredService<ISettingsService>();
                var localizationService = Services.GetRequiredService<ILocalizationService>();
                localizationService.SetLanguage(settingsService.Current.Language);

                // Apply saved Dark/Light Theme and Windows Desktop Accent Color via shared theme service
                var themeService = Services.GetRequiredService<IWindowsThemeService>();
                themeService.SetTheme(settingsService.Current.IsDarkMode);
                themeService.ApplySystemAccentColor();

                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogFatalException("OnStartup", ex);
                MessageBox.Show(
                    $"Falha ao inicializar o BITSHIFT:\n\n{ex.Message}",
                    "BITSHIFT — Erro de Inicialização",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogFatalException("DispatcherUnhandledException", e.Exception);
            MessageBox.Show(
                $"Erro inesperado na interface do BITSHIFT:\n\n{e.Exception.Message}",
                "BITSHIFT — Erro de Execução",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogFatalException("DomainUnhandledException", ex);
            }
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogFatalException("UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private static void LogFatalException(string source, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Bitshift");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "crash.log");
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex}\n\n";
                File.AppendAllText(logPath, entry);
                Debug.WriteLine(entry);
            }
            catch
            {
                // Ignore logging failures during critical errors
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Services.GetService<ITrayIconService>()?.Dispose();
            if (Services is IDisposable disposableServices)
            {
                disposableServices.Dispose();
            }
            base.OnExit(e);
        }
    }
}
