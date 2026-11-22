namespace Bitshift.Models
{
    public sealed record ScriptExecutionResult(
        bool Success,
        int ExitCode,
        string Output,
        string Error);
}
