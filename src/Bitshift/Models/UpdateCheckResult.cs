namespace Bitshift.Models
{
    public enum UpdateCheckStatus
    {
        Idle,
        Checking,
        UpToDate,
        UpdateAvailable,
        Error
    }

    public sealed record UpdateCheckResult(
        UpdateCheckStatus Status,
        string CurrentVersion,
        string LatestVersion,
        string ReleaseUrl,
        long? LatencyMs,
        string? ErrorMessage);
}
