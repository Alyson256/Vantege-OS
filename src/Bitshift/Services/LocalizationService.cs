using System;
using System.Collections.Generic;
using Bitshift.Services.Localization;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public class LocalizationService : ILocalizationService
    {
        private string _currentLanguage = "pt";
        private readonly Dictionary<string, Dictionary<string, string>> _translations;

        public string CurrentLanguage => _currentLanguage;
        public event Action? LanguageChanged;

        public LocalizationService()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>();
            CommonTranslations.Register(_translations);
            DashboardTranslations.Register(_translations);
            SpecsTranslations.Register(_translations);
            TweaksTranslations.Register(_translations);
            SettingsTranslations.Register(_translations);
        }

        public string Get(string key)
        {
            if (_translations.TryGetValue(key, out var entry) &&
                entry.TryGetValue(_currentLanguage, out var value))
            {
                return value;
            }
            return key;
        }

        public void SetLanguage(string language)
        {
            if (language != "pt" && language != "en") return;
            _currentLanguage = language;
            LanguageChanged?.Invoke();
        }
    }
}
