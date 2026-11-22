using System;
using System.IO;
using System.Text.Json;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public AppSettings Current { get; private set; }
        public event Action<AppSettings>? SettingsChanged;

        public SettingsService()
        {
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Bitshift");
            _settingsPath = Path.Combine(appDataDir, "bitshift_settings.json");
            Current = LoadFromDisk();
        }

        public void Update(Action<AppSettings> applyChanges)
        {
            applyChanges(Current);
            SaveToDisk();
            SettingsChanged?.Invoke(Current);
        }

        public void ResetToDefaults()
        {
            Current = new AppSettings();
            SaveToDisk();
            SettingsChanged?.Invoke(Current);
        }

        private AppSettings LoadFromDisk()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        if (loaded.Language != "pt" && loaded.Language != "en")
                        {
                            loaded.Language = "pt";
                        }
                        return loaded;
                    }
                }
            }
            catch
            {
                // Fall back safely to defaults if settings file is corrupted or unreadable
            }

            return new AppSettings();
        }

        private void SaveToDisk()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(Current, _jsonOptions);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Keep app running even if disk write fails due to permissions
            }
        }
    }
}
