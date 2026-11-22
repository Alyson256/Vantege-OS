using System.Diagnostics;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockClipboardService : IClipboardService
    {
        public bool TryCopy(string text)
        {
            Debug.WriteLine($"[MockClipboardService] Clipboard write skipped ({text.Length} characters).");
            return false;
        }
    }
}
