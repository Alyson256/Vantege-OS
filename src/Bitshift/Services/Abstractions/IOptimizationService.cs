using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface IOptimizationService
    {
        Task<bool> OptimizeRamAsync();
        Task<RamOptimizationResult> OptimizeRamDetailedAsync();
        Task<Dictionary<string, double>> ScanCleanerCategoriesAsync(int unusedDaysThreshold = 30, CancellationToken cancellationToken = default);
        Task<CleanerExecutionResult> CleanSelectedCategoriesAsync(IReadOnlyCollection<string> categories, int unusedDaysThreshold = 30, CancellationToken cancellationToken = default);
        Task<PowerProfileExecutionResult> ApplyPowerProfileAsync(string profileId, string? requestedGuid = null, CancellationToken cancellationToken = default);
        Task<string> DetectActivePowerProfileAsync(CancellationToken cancellationToken = default);
    }
}
