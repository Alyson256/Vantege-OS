using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VantageOS.Services
{
    public interface IOptimizationService
    {
        Task<bool> OptimizeRamAsync();
        Task<bool> CleanJunkFilesAsync();
    }

    public class OptimizationService : IOptimizationService
    {
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        public async Task<bool> OptimizeRamAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Empty working set for all processes
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        try
                        {
                            EmptyWorkingSet(process.Handle);
                        }
                        catch
                        {
                            // Ignore access denied for some processes
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> CleanJunkFilesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Basic safe temp file cleanup
                    string tempPath = Path.GetTempPath();
                    DirectoryInfo di = new DirectoryInfo(tempPath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        try { file.Delete(); } catch { }
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        try { dir.Delete(true); } catch { }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
