using System.Windows;
using VantageOS.ViewModels;

namespace VantageOS
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}