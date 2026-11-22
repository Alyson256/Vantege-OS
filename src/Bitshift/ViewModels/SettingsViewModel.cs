using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using Bitshift.Services;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, IDisposable
    {
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        private readonly IWindowsThemeService _themeService;
        private bool _isSyncing;
        private bool _disposed;

        [ObservableProperty]
        private string pageTitle = "Configurações";

        [ObservableProperty]
        private string pageDesc = string.Empty;

        [ObservableProperty]
        private string traySectionTitle = "Bandeja do Sistema";

        [ObservableProperty]
        private string minimizeToTrayTitle = "Minimizar para a bandeja do sistema";

        [ObservableProperty]
        private string minimizeToTrayDesc = string.Empty;

        [ObservableProperty]
        private string showTrayNotificationTitle = "Exibir notificação ao minimizar";

        [ObservableProperty]
        private string showTrayNotificationDesc = string.Empty;

        [ObservableProperty]
        private string generalSectionTitle = "Preferências Gerais";

        [ObservableProperty]
        private string languageTitle = "Idioma da Interface";

        [ObservableProperty]
        private string languageDesc = string.Empty;

        [ObservableProperty]
        private string accentTitle = "Cor de Destaque do Windows";

        [ObservableProperty]
        private string accentDesc = string.Empty;

        [ObservableProperty]
        private string accentColorHex = "#0078D4";

        [ObservableProperty]
        private string reloadAccentLabel = "Recarregar Cor";

        [ObservableProperty]
        private string resetDefaultsLabel = "Restaurar Padrões";

        [ObservableProperty]
        private string testMinimizeLabel = "Minimizar Agora";

        [ObservableProperty]
        private string themeTitle = "Tema da Aparência";

        [ObservableProperty]
        private string themeDesc = string.Empty;

        [ObservableProperty]
        private string themeStatusLabel = "Escuro";

        [ObservableProperty]
        private bool minimizeToTray;

        [ObservableProperty]
        private bool showTrayNotificationOnMinimize;

        [ObservableProperty]
        private string autoCheckUpdatesTitle = "Verificar atualizações ao iniciar";

        [ObservableProperty]
        private string autoCheckUpdatesDesc = string.Empty;

        [ObservableProperty]
        private bool autoCheckUpdatesOnStartup = true;

        [ObservableProperty]
        private string selectedLanguage = "pt";

        [ObservableProperty]
        private bool isPortuguese = true;

        [ObservableProperty]
        private bool isEnglish;

        [ObservableProperty]
        private bool isDarkMode = true;

        public SettingsViewModel(
            ISettingsService settingsService,
            ILocalizationService localizationService,
            IWindowsThemeService themeService)
        {
            _settingsService = settingsService;
            _localizationService = localizationService;
            _themeService = themeService;

            SyncFromSettings();
            _localizationService.LanguageChanged += OnLanguageChanged;
            _themeService.ThemeChanged += OnThemeChanged;
            UpdateLocalizedTexts();
        }

        partial void OnMinimizeToTrayChanged(bool value)
        {
            if (_isSyncing) return;
            _settingsService.Update(s => s.MinimizeToTray = value);
        }

        partial void OnShowTrayNotificationOnMinimizeChanged(bool value)
        {
            if (_isSyncing) return;
            _settingsService.Update(s => s.ShowTrayNotificationOnMinimize = value);
        }

        partial void OnAutoCheckUpdatesOnStartupChanged(bool value)
        {
            if (_isSyncing) return;
            _settingsService.Update(s => s.AutoCheckUpdatesOnStartup = value);
        }

        [RelayCommand]
        private void SetLanguage(string langCode)
        {
            if (langCode != "pt" && langCode != "en") return;
            _settingsService.Update(s => s.Language = langCode);
            _localizationService.SetLanguage(langCode);
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
            _settingsService.Update(s => s.IsDarkMode = _themeService.IsDarkMode);
        }

        [RelayCommand]
        private void ReloadAccentColor()
        {
            _themeService.ApplySystemAccentColor();
        }

        [RelayCommand]
        private void MinimizeToTrayNow()
        {
            if (!MinimizeToTray)
            {
                MinimizeToTray = true;
            }

            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void ResetDefaults()
        {
            _settingsService.ResetToDefaults();
            _themeService.SetTheme(_settingsService.Current.IsDarkMode);
            _themeService.ApplySystemAccentColor();
            _localizationService.SetLanguage(_settingsService.Current.Language);
            SyncFromSettings();
        }

        private void SyncFromSettings()
        {
            _isSyncing = true;
            try
            {
                MinimizeToTray = _settingsService.Current.MinimizeToTray;
                ShowTrayNotificationOnMinimize = _settingsService.Current.ShowTrayNotificationOnMinimize;
                AutoCheckUpdatesOnStartup = _settingsService.Current.AutoCheckUpdatesOnStartup;
                SelectedLanguage = _localizationService.CurrentLanguage;
                IsPortuguese = SelectedLanguage == "pt";
                IsEnglish = SelectedLanguage == "en";
                IsDarkMode = _themeService.IsDarkMode;
                AccentColorHex = _themeService.AccentColorHex;
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void OnThemeChanged()
        {
            SyncFromSettings();
            UpdateLocalizedTexts();
        }

        private void OnLanguageChanged()
        {
            SyncFromSettings();
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = _localizationService.Get("settings");
            PageDesc = _localizationService.Get("settingsDesc");
            TraySectionTitle = _localizationService.Get("settingsTraySection");
            MinimizeToTrayTitle = _localizationService.Get("settingsMinimizeToTray");
            MinimizeToTrayDesc = _localizationService.Get("settingsMinimizeToTrayDesc");
            ShowTrayNotificationTitle = _localizationService.Get("settingsTrayNotify");
            ShowTrayNotificationDesc = _localizationService.Get("settingsTrayNotifyDesc");
            GeneralSectionTitle = _localizationService.Get("settingsGeneralSection");
            LanguageTitle = _localizationService.Get("settingsLanguageTitle");
            LanguageDesc = _localizationService.Get("settingsLanguageDesc");
            ThemeTitle = _localizationService.Get("settingsThemeTitle");
            ThemeDesc = _localizationService.Get("settingsThemeDesc");
            ThemeStatusLabel = _localizationService.Get(IsDarkMode ? "dark" : "light");
            AccentTitle = _localizationService.Get("settingsAccentTitle");
            AccentDesc = _localizationService.Get("settingsAccentDesc");
            ReloadAccentLabel = _localizationService.Get("settingsReloadAccent");
            ResetDefaultsLabel = _localizationService.Get("settingsResetDefaults");
            TestMinimizeLabel = _localizationService.Get("settingsTestMinimize");
            AutoCheckUpdatesTitle = _localizationService.Get("settingsAutoUpdate");
            AutoCheckUpdatesDesc = _localizationService.Get("settingsAutoUpdateDesc");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _localizationService.LanguageChanged -= OnLanguageChanged;
            _themeService.ThemeChanged -= OnThemeChanged;
            GC.SuppressFinalize(this);
        }
    }
}
