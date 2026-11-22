using System;

namespace Bitshift.Services.Abstractions
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        string Get(string key);
        void SetLanguage(string language);
        event Action? LanguageChanged;
    }
}
