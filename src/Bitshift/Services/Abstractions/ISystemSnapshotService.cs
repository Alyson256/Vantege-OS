using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface ISystemSnapshotService
    {
        Task<Win32HardwareSnapshotDto> GetHardwareSnapshotAsync(CancellationToken cancellationToken = default);
        Task RefreshStaticMetadataAsync(CancellationToken cancellationToken = default);
    }
}
