using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface IBatchExecutionService
    {
        event Action<string>? OutputReceived;
        Task<ScriptExecutionResult> ExecuteScriptAsync(string scriptRelativeName, string? arguments = null, CancellationToken cancellationToken = default);
        Task<ScriptExecutionResult> ExecuteBatchContentAsync(string batchContent, bool runAsAdmin = false, CancellationToken cancellationToken = default);
        Task<ScriptExecutionResult> ExecuteCommandAsync(string command, string? arguments = null, bool runAsAdmin = false, CancellationToken cancellationToken = default);
        Task<ScriptExecutionResult> ExecutePowerShellAsync(string powerShellScript, bool runAsAdmin = false, CancellationToken cancellationToken = default);
        string GetScriptsDirectory();
        bool IsAdministrator();
    }
}
