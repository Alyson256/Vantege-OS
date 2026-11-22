using System;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface ISettingsService
    {
        AppSettings Current { get; }
        event Action<AppSettings>? SettingsChanged;
        void Update(Action<AppSettings> applyChanges);
        void ResetToDefaults();
    }
}
