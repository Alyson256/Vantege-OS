using System.Diagnostics;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockLauncher : ILauncher
    {
        public void OpenUrl(string url)
        {
            Debug.WriteLine($"[MockLauncher] Would open URL: {url}");
        }

        public bool RelaunchAsAdministrator()
        {
            Debug.WriteLine("[MockLauncher] Administrator restart skipped; UAC is simulated as denied.");
            return false;
        }
    }
}
