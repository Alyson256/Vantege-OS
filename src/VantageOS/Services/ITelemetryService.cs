using System;
using System.Threading;
using System.Threading.Tasks;
using VantageOS.Models;

namespace VantageOS.Services
{
    public interface ITelemetryService
    {
        Task<TelemetrySnapshot> GetSnapshotAsync();
        
        // Permite subscrever para receber atualizações contínuas
        void StartMonitoring(Action<TelemetrySnapshot> onSnapshotUpdated, CancellationToken token);
    }
}
