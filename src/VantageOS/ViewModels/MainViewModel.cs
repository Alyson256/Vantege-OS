using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VantageOS.Services;

namespace VantageOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
                private readonly ILocalizationService _localizationService;
        [ObservableProperty]
        private string title = "Vantage OS Core";
        
        [ObservableProperty]
        private ObservableObject currentViewModel;

        [ObservableProperty]
        private string languageLabel = "Idioma";

        [ObservableProperty]
        private string dashboardLabel = "Dashboard";

        [ObservableProperty]
        private string appsLabel = "Apps";

        [ObservableProperty]
        private string networkLabel = "Network & DNS";

        [ObservableProperty]
        private string profilesLabel = "Perfis de Uso";

        public MainViewModel(DashboardViewModel dashboardViewModel, ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts(); // Set initial texts
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
            AppsLabel = _localizationService.Get("appsTab");
            NetworkLabel = _localizationService.Get("networkTab");
            ProfilesLabel = _localizationService.Get("profiles");
        }

        [RelayCommand]
        private void ChangeLanguage(string langCode)
        {
            _localizationService.SetLanguage(langCode);
        }
    }
}
