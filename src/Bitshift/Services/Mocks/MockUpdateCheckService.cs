using System;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockUpdateCheckService : IUpdateCheckService
    {
        private const string AppVersion = "2.1.0";
        private readonly MockServiceOptions _options;

        public string CurrentVersion => AppVersion;
        public string RepositoryUrl => "https://github.com/Alyson256/Bitshift";

        public MockUpdateCheckService(MockServiceOptions options)
        {
            _options = options;
        }

        public async Task<UpdateCheckResult> PingAsync(CancellationToken cancellationToken = default)
        {
            var startedAt = DateTime.UtcNow;
            await _options.SimulateDelayAsync(500, cancellationToken);
            long elapsedMilliseconds = Math.Max(1, (long)(DateTime.UtcNow - startedAt).TotalMilliseconds);

            if (_options.IsError)
            {
                return new UpdateCheckResult(
                    UpdateCheckStatus.Error,
                    AppVersion,
                    AppVersion,
                    $"{RepositoryUrl}/releases",
                    elapsedMilliseconds,
                    "Falha de rede simulada.");
            }

            bool updateAvailable = _options.UpdateAvailable && !_options.IsEmpty;
            string latestVersion = updateAvailable ? "2.2.0" : AppVersion;
            string releaseUrl = updateAvailable
                ? $"{RepositoryUrl}/releases/tag/v{latestVersion}"
                : $"{RepositoryUrl}/releases";

            return new UpdateCheckResult(
                updateAvailable ? UpdateCheckStatus.UpdateAvailable : UpdateCheckStatus.UpToDate,
                AppVersion,
                latestVersion,
                releaseUrl,
                elapsedMilliseconds,
                null);
        }
    }
}
