namespace Bitshift.Models
{
    public class AppSettings
    {
        public bool MinimizeToTray { get; set; } = true;
        public bool ShowTrayNotificationOnMinimize { get; set; } = true;
        public bool AutoCheckUpdatesOnStartup { get; set; } = true;
        public string Language { get; set; } = "pt";
        public bool IsDarkMode { get; set; } = true;
    }
}
