using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Bitshift.Services.Abstractions;

using Wpf.Ui.Appearance;

namespace Bitshift.Services
{
    public class WindowsThemeService : IWindowsThemeService
    {
        public Color CurrentAccentColor { get; private set; } = Color.FromRgb(0x00, 0x78, 0xD4); // Windows 11 Fluent Blue default
        public string AccentColorHex => $"#{CurrentAccentColor.R:X2}{CurrentAccentColor.G:X2}{CurrentAccentColor.B:X2}";
        public bool IsDetectedFromWindows { get; private set; }
        public bool IsDarkMode { get; private set; } = true;
        public event Action? ThemeChanged;

        public void ApplySystemAccentColor()
        {
            var detectedColor = ReadWindowsAccentColor();
            if (detectedColor.HasValue)
            {
                CurrentAccentColor = detectedColor.Value;
                IsDetectedFromWindows = true;
            }
            else
            {
                CurrentAccentColor = Color.FromRgb(0x00, 0x78, 0xD4); // Windows 11 Fluent default
                IsDetectedFromWindows = false;
            }

            UpdateApplicationBrushes(CurrentAccentColor);
            ThemeChanged?.Invoke();
        }

        public void SetCustomAccentColor(Color color)
        {
            CurrentAccentColor = color;
            IsDetectedFromWindows = false;
            UpdateApplicationBrushes(color);
            ThemeChanged?.Invoke();
        }

        public void SetTheme(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
            if (Application.Current != null)
            {
                try
                {
                    ApplicationThemeManager.Apply(isDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WindowsThemeService] Falha ao aplicar tema WPF-UI: {ex.Message}");
                }

                UpdateApplicationBrushes(CurrentAccentColor);
            }
            ThemeChanged?.Invoke();
        }

        public void ToggleTheme()
        {
            SetTheme(!IsDarkMode);
        }

        private Color? ReadWindowsAccentColor()
        {
            try
            {
                // Method 1: Check DWM AccentColor (DWORD format: 0xAABBGGRR)
                using var dwmKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
                if (dwmKey != null)
                {
                    var accentVal = dwmKey.GetValue("AccentColor");
                    if (accentVal is int intVal)
                    {
                        byte r = (byte)(intVal & 0xFF);
                        byte g = (byte)((intVal >> 8) & 0xFF);
                        byte b = (byte)((intVal >> 16) & 0xFF);
                        return Color.FromRgb(r, g, b);
                    }

                    var colorizationVal = dwmKey.GetValue("ColorizationColor");
                    if (colorizationVal is int colVal)
                    {
                        // ColorizationColor is usually ARGB (0xAARRGGBB)
                        byte r = (byte)((colVal >> 16) & 0xFF);
                        byte g = (byte)((colVal >> 8) & 0xFF);
                        byte b = (byte)(colVal & 0xFF);
                        return Color.FromRgb(r, g, b);
                    }
                }

                // Method 2: Check Windows Explorer Accent palette
                using var accentKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent");
                if (accentKey != null)
                {
                    var menuVal = accentKey.GetValue("AccentColorMenu");
                    if (menuVal is int intVal)
                    {
                        byte r = (byte)(intVal & 0xFF);
                        byte g = (byte)((intVal >> 8) & 0xFF);
                        byte b = (byte)((intVal >> 16) & 0xFF);
                        return Color.FromRgb(r, g, b);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WindowsThemeService] Falha ao ler registro DWM: {ex.Message}");
            }

            return null;
        }

        private void UpdateApplicationBrushes(Color accent)
        {
            if (Application.Current == null) return;

            // Generate harmonious dark mode variants
            byte r = accent.R;
            byte g = accent.G;
            byte b = accent.B;

            // Solid brush
            var solidBrush = new SolidColorBrush(Color.FromRgb(r, g, b));
            solidBrush.Freeze();

            // Hover brush (slightly brighter/saturated for tactile mouse hover)
            static byte Clamp(int val) => (byte)Math.Max(0, Math.Min(255, val));
            var hoverColor = Color.FromRgb(Clamp(r + 28), Clamp(g + 28), Clamp(b + 28));
            var hoverBrush = new SolidColorBrush(hoverColor);
            hoverBrush.Freeze();

            // Subtle brush (Dark mode background container: blended 18% over dark #121218)
            var subtleColor = Color.FromArgb(45, r, g, b);
            var subtleBrush = new SolidColorBrush(subtleColor);
            subtleBrush.Freeze();

            // Border brush (semi-transparent 45% accent)
            var borderColor = Color.FromArgb(120, r, g, b);
            var borderBrush = new SolidColorBrush(borderColor);
            borderBrush.Freeze();

            // Text tint brush (bright, readable pastel tone on dark backgrounds)
            // Lighter version of the accent to ensure high contrast WCAG compliance
            var textColor = Color.FromRgb(
                Clamp((int)(r * 0.55 + 255 * 0.45)),
                Clamp((int)(g * 0.55 + 255 * 0.45)),
                Clamp((int)(b * 0.55 + 255 * 0.45))
            );
            var textBrush = new SolidColorBrush(textColor);
            textBrush.Freeze();

            // Apply standard Accent brushes
            Application.Current.Resources["SystemAccentBrush"] = solidBrush;
            Application.Current.Resources["AccentBrush"] = solidBrush;
            Application.Current.Resources["AccentSolidBrush"] = solidBrush;
            Application.Current.Resources["AccentHoverBrush"] = hoverBrush;
            Application.Current.Resources["AccentSubtleBrush"] = subtleBrush;
            Application.Current.Resources["AccentBorderBrush"] = borderBrush;
            Application.Current.Resources["AccentTextBrush"] = textBrush;
        }
    }
}
