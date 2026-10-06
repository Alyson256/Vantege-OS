using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using VantageOS.ViewModels;
using VantageOS.Services;

namespace VantageOS
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<ITelemetryService, WmiTelemetryService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IOptimizationService, OptimizationService>();
            services.AddSingleton<ISystemSpecService, SystemSpecService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<SystemSpecsViewModel>();
            
            // Views
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
