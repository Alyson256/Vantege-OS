using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface ITweakService
    {
        IReadOnlyList<TweakSummary> GetAllTweaks();
        Task<ScriptExecutionResult> ExecuteTweakAsync(string tweakId, CancellationToken cancellationToken = default);
        Task<ScriptExecutionResult> UndoAllAsync(CancellationToken cancellationToken = default);
    }
}
