using CommunityToolkit.Mvvm.ComponentModel;
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
        private string language = "pt";

        public MainViewModel(DashboardViewModel dashboardViewModel, ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            Language = _localizationService.Get("language"); // initial language code
            CurrentViewModel = dashboardViewModel;
        }
        private void OnLanguageChanged()
        {
            Language = _localizationService.Get("language");
        }
    }
}
