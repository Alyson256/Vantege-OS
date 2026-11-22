using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockSystemSnapshotService : ISystemSnapshotService
    {
        private readonly MockServiceOptions _options;

        public MockSystemSnapshotService(MockServiceOptions options)
        {
            _options = options;
        }

        public async Task RefreshStaticMetadataAsync(CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(500, cancellationToken);
            _options.ThrowIfError();
        }

        public async Task<Win32HardwareSnapshotDto> GetHardwareSnapshotAsync(CancellationToken cancellationToken = default)
        {
            await _options.SimulateDelayAsync(350, cancellationToken);
            _options.ThrowIfError();
            return MockHardwareData.CreateSnapshot(_options.IsEmpty);
        }
    }
}
