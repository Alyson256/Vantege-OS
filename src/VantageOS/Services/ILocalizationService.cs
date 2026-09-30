namespace VantageOS.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        string Get(string key);
        void SetLanguage(string language);
        event Action? LanguageChanged;
    }
}
