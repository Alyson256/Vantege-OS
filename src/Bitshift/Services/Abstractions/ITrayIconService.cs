using System;
using System.Threading.Tasks;

namespace Bitshift.Services.Abstractions
{
    public interface ITrayIconService : IDisposable
    {
        bool IsVisible { get; }
        void Initialize(Action onOpenRequested, Func<Task> onCleanRamRequested, Action onExitRequested);
        void SetVisible(bool visible);
        void ShowNotification(string title, string message, bool isError = false);
    }
}
