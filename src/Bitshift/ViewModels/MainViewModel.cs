using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bitshift.Models;
using Bitshift.Services;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly ILocalizationService _localizationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowsThemeService _themeService;
        private readonly IBatchExecutionService _batchService;
        private readonly ISettingsService _settingsService;
        private readonly IUpdateCheckService _updateCheckService;
        private readonly ILauncher _launcher;
        private readonly IDialogService _dialogService;
        private readonly CancellationTokenSource _lifecycleCts = new();
        private UpdateCheckStatus _lastUpdateStatus = UpdateCheckStatus.Idle;
        private string _lastLatestVersion = "2.1.0";
        private bool _disposed;

        [ObservableProperty]
        private string title = "BITSHIFT";

        [ObservableProperty]
        private ObservableObject currentViewModel;

        [ObservableProperty]
        private string dashboardLabel = "Dashboard";

        [ObservableProperty]
        private string pcBoostLabel = "PC Boost";

        [ObservableProperty]
        private string specsLabel = "Especificações";

        [ObservableProperty]
        private string tweaksLabel = "Customização";

        [ObservableProperty]
        private string settingsLabel = "Configurações";

        [ObservableProperty]
        private string aboutLabel = "Sobre";

        [ObservableProperty]
        private string updatesLabel = "Verificar atualizações";

        [ObservableProperty]
        private string brandTaglineLabel = "seu sistema suas regras";

        [ObservableProperty]
        private string updateStatusLabel = "Atualizado";

        [ObservableProperty]
        private string updateMetaLabel = "v2.1.0";

        [ObservableProperty]
        private string updateStatusCode = "up_to_date";

        [ObservableProperty]
        private bool hasUpdateAvailable;

        [ObservableProperty]
        private bool isCheckingUpdates;

        [ObservableProperty]
        private string openReleaseLabel = "Baixar no GitHub";


        [ObservableProperty]
        private string sidebarProfileLabel = "Perfil";

        [ObservableProperty]
        private string sidebarColorLabel = "Cor";

        [ObservableProperty]
        private string sidebarAdminBtnLabel = "Modo ADM (UAC)";

        [ObservableProperty]
        private string latestReleaseUrl = "https://github.com/Alyson256/Bitshift/releases";

        [ObservableProperty]
        private string activeView = "dashboard";

        [ObservableProperty]
        private string accentColorHex = "#0078D4";

        [ObservableProperty]
        private bool isSidebarCollapsed;

        [ObservableProperty]
        private bool isAdministratorMode;

        [ObservableProperty]
        private string appStatusText = "Modo Seguro (Ring-3)";

        [ObservableProperty]
        private bool isPortuguese = true;

        [ObservableProperty]
        private bool isEnglish;

        [ObservableProperty]
        private bool isDarkMode = true;

        [ObservableProperty]
        private string themeStatusLabel = "Escuro";

        [ObservableProperty]
        private string currentVersion = "v2.5.0";

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            ILocalizationService localizationService,
            IServiceProvider serviceProvider,
            IWindowsThemeService themeService,
            IBatchExecutionService batchService,
            ISettingsService settingsService,
            IUpdateCheckService updateCheckService,
            ILauncher launcher,
            IDialogService dialogService)
        {
            _localizationService = localizationService;
            _serviceProvider = serviceProvider;
            _themeService = themeService;
            _batchService = batchService;
            _settingsService = settingsService;
            _updateCheckService = updateCheckService;
            _launcher = launcher;
            _dialogService = dialogService;
            IsAdministratorMode = _batchService.IsAdministrator();
            CurrentVersion = $"v{_updateCheckService.CurrentVersion}";
            LatestReleaseUrl = $"{_updateCheckService.RepositoryUrl}/releases";

            _localizationService.LanguageChanged += OnLanguageChanged;
            _themeService.ThemeChanged += OnThemeChanged;
            SyncThemeAndLanguageState();
            UpdateLocalizedTexts();
            CurrentViewModel = dashboardViewModel;

            if (_settingsService.Current.AutoCheckUpdatesOnStartup)
            {
                _ = CheckUpdatesAsync();
            }
        }

        [RelayCommand]
        private void ReloadSystemAccentColor()
        {
            _themeService.ApplySystemAccentColor();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
            _settingsService.Update(s => s.IsDarkMode = _themeService.IsDarkMode);
        }

        private void OnThemeChanged()
        {
            SyncThemeAndLanguageState();
            UpdateLocalizedTexts();
        }

        private void OnLanguageChanged()
        {
            SyncThemeAndLanguageState();
            UpdateLocalizedTexts();
        }

        private void SyncThemeAndLanguageState()
        {
            IsPortuguese = _localizationService.CurrentLanguage == "pt";
            IsEnglish = _localizationService.CurrentLanguage == "en";
            IsDarkMode = _themeService.IsDarkMode;
            AccentColorHex = _themeService.AccentColorHex;
        }

        private void UpdateLocalizedTexts()
        {
            DashboardLabel = _localizationService.Get("dashboard");
            PcBoostLabel = "PC Boost";
            SpecsLabel = _localizationService.Get("systemSpecs");
            TweaksLabel = _localizationService.Get("custom");
            SettingsLabel = _localizationService.Get("settings");
            AboutLabel = _localizationService.Get("about");
            UpdatesLabel = _localizationService.Get("checkUpdates");
            BrandTaglineLabel = _localizationService.Get("brandTagline");
            SidebarProfileLabel = _localizationService.Get("sidebarProfile");
            SidebarColorLabel = _localizationService.Get("sidebarColor");
            SidebarAdminBtnLabel = _localizationService.Get("sidebarAdminBtn");
            ThemeStatusLabel = _localizationService.Get(IsDarkMode ? "dark" : "light");
            AppStatusText = IsAdministratorMode
                ? _localizationService.Get("modeAdmin")
                : _localizationService.Get("modeStandard");
            RefreshUpdateLabels();
        }

        private void RefreshUpdateLabels()
        {
            UpdateStatusCode = _lastUpdateStatus switch
            {
                UpdateCheckStatus.Checking => "checking",
                UpdateCheckStatus.UpdateAvailable => "update_available",
                UpdateCheckStatus.Error => "error",
                _ => "up_to_date"
            };

            UpdateStatusLabel = _lastUpdateStatus switch
            {
                UpdateCheckStatus.Checking => _localizationService.Get("checkingUpdates"),
                UpdateCheckStatus.UpdateAvailable => _localizationService.Get("updateAvailable"),
                UpdateCheckStatus.Error => _localizationService.Get("updateError"),
                _ => _localizationService.Get("upToDate")
            };

            OpenReleaseLabel = _lastUpdateStatus == UpdateCheckStatus.UpdateAvailable
                ? $"{_localizationService.Get("openRelease")} (v{_lastLatestVersion})"
                : _localizationService.Get("openRelease");

            UpdateMetaLabel = $"v{_updateCheckService.CurrentVersion}";
        }

        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            if (IsCheckingUpdates || _disposed) return;

            IsCheckingUpdates = true;
            _lastUpdateStatus = UpdateCheckStatus.Checking;
            RefreshUpdateLabels();

            try
            {
                UpdateCheckResult result = await _updateCheckService.PingAsync(_lifecycleCts.Token);
                if (_disposed) return;

                _lastUpdateStatus = result.Status;
                _lastLatestVersion = result.LatestVersion;
                LatestReleaseUrl = result.ReleaseUrl;
                HasUpdateAvailable = result.Status == UpdateCheckStatus.UpdateAvailable;
                RefreshUpdateLabels();
            }
            catch
            {
                if (_disposed) return;
                _lastUpdateStatus = UpdateCheckStatus.Error;
                HasUpdateAvailable = false;
                RefreshUpdateLabels();
            }
            finally
            {
                if (!_disposed)
                {
                    IsCheckingUpdates = false;
                }
            }
        }

        [RelayCommand]
        private void OpenLatestRelease()
        {
            try
            {
                _launcher.OpenUrl(LatestReleaseUrl);
            }
            catch (Exception ex)
            {
                _dialogService.Show("BITSHIFT", ex.Message, DialogMessageType.Error);
            }
        }

        [RelayCommand]
        private void OpenKofi()
        {
            try
            {
                _launcher.OpenUrl("https://ko-fi.com/alyson256");
            }
            catch (Exception ex)
            {
                _dialogService.Show("BITSHIFT", ex.Message, DialogMessageType.Error);
            }
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
        }

        [RelayCommand]
        private void ToggleAdministratorMode()
        {
            if (_batchService.IsAdministrator())
            {
                IsAdministratorMode = true;
                AppStatusText = _localizationService.Get("modeAdmin");
                _dialogService.Show(
                    "Privilégios UAC",
                    "O Bitshift já está sendo executado com privilégios de Administrador.",
                    DialogMessageType.Information);
                return;
            }

            try
            {
                if (_launcher.RelaunchAsAdministrator())
                {
                    Application.Current?.Shutdown();
                }
                else
                {
                    IsAdministratorMode = _batchService.IsAdministrator();
                    AppStatusText = _localizationService.Get("modeStandard");
                    _dialogService.Show(
                        "Privilégios UAC",
                        "A solicitação de elevação foi negada ou cancelada.",
                        DialogMessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                IsAdministratorMode = _batchService.IsAdministrator();
                AppStatusText = _localizationService.Get("modeStandard");
                _dialogService.Show("Privilégios UAC", ex.Message, DialogMessageType.Error);
            }
        }

        [RelayCommand]
        private void ChangeLanguage(string langCode)
        {
            _settingsService.Update(s => s.Language = langCode);
            _localizationService.SetLanguage(langCode);
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            ObservableObject? nextViewModel = viewName switch
            {
                "dashboard" => _serviceProvider.GetRequiredService<DashboardViewModel>(),
                "pcboost" => _serviceProvider.GetRequiredService<PcBoostViewModel>(),
                "specs" => _serviceProvider.GetRequiredService<SystemSpecsViewModel>(),
                "tweaks" => _serviceProvider.GetRequiredService<CustomTweaksViewModel>(),
                "settings" => _serviceProvider.GetRequiredService<SettingsViewModel>(),
                "about" => _serviceProvider.GetRequiredService<AboutViewModel>(),
                _ => null
            };

            if (nextViewModel == null) return;

            if (!ReferenceEquals(CurrentViewModel, nextViewModel) && CurrentViewModel is IDisposable oldViewModel)
            {
                oldViewModel.Dispose();
            }

            ActiveView = viewName;
            CurrentViewModel = nextViewModel;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _lifecycleCts.Cancel();
            _lifecycleCts.Dispose();
            _localizationService.LanguageChanged -= OnLanguageChanged;
            _themeService.ThemeChanged -= OnThemeChanged;
            GC.SuppressFinalize(this);
        }
    }
}
