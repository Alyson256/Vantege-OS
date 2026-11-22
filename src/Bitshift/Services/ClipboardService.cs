using System.Runtime.InteropServices;
using System.Windows;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public sealed class ClipboardService : IClipboardService
    {
        public bool TryCopy(string text)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (ExternalException)
            {
                return false;
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
        }
    }
}
