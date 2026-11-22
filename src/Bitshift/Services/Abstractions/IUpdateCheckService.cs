using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface IUpdateCheckService
    {
        string CurrentVersion { get; }
        string RepositoryUrl { get; }
        Task<UpdateCheckResult> PingAsync(CancellationToken cancellationToken = default);
    }
}
