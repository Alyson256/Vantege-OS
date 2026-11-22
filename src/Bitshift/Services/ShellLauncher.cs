using System;
using System.ComponentModel;
using System.Diagnostics;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public sealed class ShellLauncher : ILauncher
    {
        public void OpenUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Only absolute HTTP and HTTPS URLs can be opened.", nameof(url));
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
        }

        public bool RelaunchAsAdministrator()
        {
            string executablePath = Environment.ProcessPath
                ?? throw new InvalidOperationException("The current executable path is unavailable.");

            try
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                }) != null;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return false;
            }
        }
    }
}
