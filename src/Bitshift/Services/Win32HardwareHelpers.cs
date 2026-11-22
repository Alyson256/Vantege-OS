using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace Bitshift.Services
{
    internal static class Win32HardwareHelpers
    {
        private const string DisplayClassPath = @"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

        public static bool IsSoftwareGpuAdapter(string name)
        {
            return name.Contains("Microsoft Basic", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Remote", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Virtual", StringComparison.OrdinalIgnoreCase);
        }

        public static ulong ReadGpuMemoryBytesFromRegistry(string? adapterName = null)
        {
            try
            {
                using var classKey = Registry.LocalMachine.OpenSubKey(DisplayClassPath);
                if (classKey == null) return 0;

                ulong fallbackBytes = 0;
                foreach (var subName in classKey.GetSubKeyNames())
                {
                    if (!subName.StartsWith("0", StringComparison.Ordinal)) continue;
                    using var subKey = classKey.OpenSubKey(subName);
                    if (subKey == null) continue;

                    ulong bytes = ReadQwMemorySize(subKey.GetValue("HardwareInformation.qwMemorySize"));
                    if (bytes == 0) continue;

                    if (!string.IsNullOrWhiteSpace(adapterName))
                    {
                        string? desc = subKey.GetValue("DriverDesc")?.ToString();
                        if (!string.IsNullOrWhiteSpace(desc) &&
                            string.Equals(desc.Trim(), adapterName.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            return bytes;
                        }
                    }

                    if (bytes > fallbackBytes)
                    {
                        fallbackBytes = bytes;
                    }
                }

                return fallbackBytes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Win32HardwareHelpers] Registry VRAM read error: {ex.Message}");
                return 0;
            }
        }

        public static ulong ReadQwMemorySize(object? rawVal)
        {
            if (rawVal is long l && l > 0) return (ulong)l;
            if (rawVal is int i && i > 0) return (ulong)i;
            if (rawVal is byte[] bytes && bytes.Length >= 8)
            {
                return BitConverter.ToUInt64(bytes, 0);
            }
            return 0;
        }

        public static (string Name, string SpeedLabel, long Mbps) GetActiveNetworkAdapterSummary()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up
                                && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();

                if (nic != null)
                {
                    long mbps = nic.Speed > 0 ? nic.Speed / 1_000_000 : 0;
                    string speedText = mbps > 0 ? $"{mbps} Mbps ({nic.NetworkInterfaceType})" : nic.NetworkInterfaceType.ToString();
                    return (nic.Description, speedText, mbps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Win32HardwareHelpers] NIC read error: {ex.Message}");
            }

            return ("—", "—", 0);
        }
    }
}
