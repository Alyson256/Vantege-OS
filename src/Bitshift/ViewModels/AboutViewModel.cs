using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string appVersion = "v2.1.0 (Produção)";

        [ObservableProperty]
        private string targetFramework = "C# 13 • .NET 10 (net10.0-windows)";

        [ObservableProperty]
        private string authorName = "Alyson Araújo";

        [ObservableProperty]
        private string repositoryUrl = "https://github.com/Alyson256/Bitshift";

        [ObservableProperty]
        private string kofiUrl = "https://ko-fi.com/alyson256";

        [ObservableProperty]
        private string kofiStatusMessage = string.Empty;

        private readonly ILauncher _launcher;
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;

        public AboutViewModel(
            ILauncher launcher,
            IClipboardService clipboardService,
            IDialogService dialogService)
        {
            _launcher = launcher;
            _clipboardService = clipboardService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void OpenRepository()
        {
            OpenUrl(RepositoryUrl);
        }

        [RelayCommand]
        private void OpenKofi()
        {
            OpenUrl(KofiUrl);
        }

        [RelayCommand]
        private void CopyKofiUrl()
        {
            if (_clipboardService.TryCopy(KofiUrl))
                KofiStatusMessage = "Link do Ko-fi copiado!";
            else
                KofiStatusMessage = KofiUrl;
        }

        private void OpenUrl(string url)
        {
            try
            {
                _launcher.OpenUrl(url);
            }
            catch (System.Exception ex)
            {
                _dialogService.Show("BITSHIFT", ex.Message, DialogMessageType.Error);
            }
        }
    }
}
