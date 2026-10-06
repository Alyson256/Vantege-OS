using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using VantageOS.Services;

namespace VantageOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string title = "Vantage OS Core";
        
        [ObservableProperty]
        private ObservableObject currentViewModel;

        [ObservableProperty]
        private string languageLabel = "Idioma";

        [ObservableProperty]
        private string dashboardLabel = "Dashboard";

        [ObservableProperty]
        private string specsLabel = "Especificações";

        [ObservableProperty]
        private string appsLabel = "Apps";

        [ObservableProperty]
        private string networkLabel = "Network & DNS";

        [ObservableProperty]
        private string profilesLabel = "Perfis de Uso";

        [ObservableProperty]
        private string activeView = "dashboard";

        public MainViewModel(DashboardViewModel dashboardViewModel, ILocalizationService localizationService, IServiceProvider serviceProvider)
        {
            _localizationService = localizationService;
            _serviceProvider = serviceProvider;
            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();
            CurrentViewModel = dashboardViewModel;
        }

        private void OnLanguageChanged()
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            LanguageLabel = _localizationService.Get("language");
            DashboardLabel = _localizationService.Get("dashboard");
            SpecsLabel = _localizationService.Get("systemSpecs");
            AppsLabel = _localizationService.Get("appsTab");
            NetworkLabel = _localizationService.Get("networkTab");
            ProfilesLabel = _localizationService.Get("profiles");
        }

        [RelayCommand]
        private void ChangeLanguage(string langCode)
        {
            _localizationService.SetLanguage(langCode);
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            ActiveView = viewName;
            CurrentViewModel = viewName switch
            {
                "dashboard" => _serviceProvider.GetRequiredService<DashboardViewModel>(),
                "specs" => _serviceProvider.GetRequiredService<SystemSpecsViewModel>(),
                _ => CurrentViewModel
            };
        }
    }
}
