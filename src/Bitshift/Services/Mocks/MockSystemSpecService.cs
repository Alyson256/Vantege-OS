using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockSystemSpecService : ISystemSpecService
    {
        private readonly MockServiceOptions _options;

        public MockSystemSpecService(MockServiceOptions options)
        {
            _options = options;
        }

        public async Task<SystemSpecInfo> GetSpecsAsync()
        {
            await _options.SimulateDelayAsync(600);
            _options.ThrowIfError();
            return MockHardwareData.CreateSpecs(_options.IsEmpty);
        }
    }
}
