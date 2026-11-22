using System;
using System.Windows.Media;

namespace Bitshift.Services.Abstractions
{
    public interface IWindowsThemeService
    {
        Color CurrentAccentColor { get; }
        string AccentColorHex { get; }
        bool IsDetectedFromWindows { get; }
        bool IsDarkMode { get; }
        event Action? ThemeChanged;
        void ApplySystemAccentColor();
        void SetCustomAccentColor(Color color);
        void SetTheme(bool isDarkMode);
        void ToggleTheme();
    }
}
