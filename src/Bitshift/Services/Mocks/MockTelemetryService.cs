using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockTelemetryService : ITelemetryService
    {
        private readonly MockServiceOptions _options;

        public MockTelemetryService(MockServiceOptions options)
        {
            _options = options;
        }

        public async Task<TelemetrySnapshot> GetSnapshotAsync()
        {
            await _options.SimulateDelayAsync(150);
            _options.ThrowIfError();
            return MockHardwareData.CreateTelemetry(_options.IsEmpty);
        }

        public void StartMonitoring(Action<TelemetrySnapshot> onSnapshotUpdated, CancellationToken token)
        {
            _ = MonitorAsync(onSnapshotUpdated, token);
        }

        private async Task MonitorAsync(Action<TelemetrySnapshot> onSnapshotUpdated, CancellationToken token)
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await timer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) break;
                    onSnapshotUpdated(await GetSnapshotAsync());
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MockTelemetryService] Monitoring failed: {ex}");
            }
        }
    }
}
