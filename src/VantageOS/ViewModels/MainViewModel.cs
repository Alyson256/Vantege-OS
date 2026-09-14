using CommunityToolkit.Mvvm.ComponentModel;

namespace VantageOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = "Vantage OS Core";
        
        [ObservableProperty]
        private ObservableObject currentViewModel;

        public MainViewModel(DashboardViewModel dashboardViewModel)
        {
            CurrentViewModel = dashboardViewModel;
        }
    }
}
