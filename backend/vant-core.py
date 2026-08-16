"""
Vantage OS - Native Core Engine
low level win32 api integration, more to add to this section.
asp asynchronous telemetry and registry tweaks engine with rollback support.

"""

import sys
import ctypes
from ctypes import wintypes, Structure, c_ulonglong, c_ulong, byref
import winreg
from typing import Dict, Any, Optional
import json

# Win32 API Declarations
Advapi32 = ctypes.WinDLL("Advapi32.dll", use_last_error=True)
Kernel32 = ctypes.WinDLL("Kernel32.dll", use_last_error=True)
Shell32 = ctypes.WinDLL("Shell32.dll", use_last_error=True)

class MEMORYSTATUSEX(Structure):
    _fields_ = [
        ("dwLength", c_ulong),
        ("dwMemoryLoad", c_ulong),
        ("ullTotalPhys", c_ulonglong),
        ("ullAvailPhys", c_ulonglong),
        ("ullTotalPageFile", c_ulonglong),
        ("ullAvailPageFile", c_ulonglong),
        ("ullTotalVirtual", c_ulonglong),
        ("ullAvailVirtual", c_ulonglong),
        ("ullAvailExtendedVirtual", c_ulonglong),
    ]

class SecurityManager:
    """Verifica e gerencia privilégios administrativos (UAC) via Token da Win32 API."""
    
    @staticmethod
    def is_admin() -> bool:
        try:
            return ctypes.windll.shell32.IsUserAnAdmin() != 0
        except Exception:
            return False

    @staticmethod
    def elevate_process() -> None:
        """Reinicia o processo solicitando elevação de privilégios com o verbo 'runas'."""
        if not SecurityManager.is_admin():
            h_instance = Shell32.ShellExecuteW(
                None, "runas", sys.executable, " ".join(sys.argv), None, 1
            )
            if h_instance <= 32:
                raise PermissionError("Falha na elevação de privilégios UAC.")
            sys.exit(0)


class HardwareTelemetry:
    """Coletor de telemetria O(1) de alta performance sem overhead de subshell."""

    def __init__(self):
        self._prev_idle_time = 0
        self._prev_kernel_time = 0
        self._prev_user_time = 0

    def get_memory_metrics(self) -> Dict[str, float]:
        """Obtém status de RAM real via GlobalMemoryStatusEx."""
        stat = MEMORYSTATUSEX()
        stat.dwLength = ctypes.sizeof(MEMORYSTATUSEX)
        if not Kernel32.GlobalMemoryStatusEx(byref(stat)):
            raise ctypes.WinError(ctypes.get_last_error())
        
        total_gb = stat.ullTotalPhys / (1024 ** 3)
        avail_gb = stat.ullAvailPhys / (1024 ** 3)
        used_gb = total_gb - avail_gb
        
        return {
            "total_gb": round(total_gb, 2),
            "used_gb": round(used_gb, 2),
            "avail_gb": round(avail_gb, 2),
            "load_percentage": stat.dwMemoryLoad
        }

    def get_cpu_load(self) -> float:
        """Calcula o uso de CPU via GetSystemTimes sem travar a thread."""
        idle_time = wintypes.FILETIME()
        kernel_time = wintypes.FILETIME()
        user_time = wintypes.FILETIME()

        if not Kernel32.GetSystemTimes(byref(idle_time), byref(kernel_time), byref(user_time)):
            return 0.0

        def ft_to_int(ft):
            return (ft.dwHighDateTime << 32) + ft.dwLowDateTime

        idle = ft_to_int(idle_time)
        kernel = ft_to_int(kernel_time)
        user = ft_to_int(user_time)

        d_idle = idle - self._prev_idle_time
        d_kernel = kernel - self._prev_kernel_time
        d_user = user - self._prev_user_time

        self._prev_idle_time = idle
        self._prev_kernel_time = kernel
        self._prev_user_time = user

        total_system = d_kernel + d_user
        if total_system == 0:
            return 0.0

        cpu_usage = (total_system - d_idle) * 100.0 / total_system
        return max(0.0, min(100.0, round(cpu_usage, 1)))

" to much to add here, i will study the workflow first, i don´t want to make thigs burn..."


class RegistryCommandEngine:
    """Engine transacional para aplicação e reversão de tweaks no Registro do Windows."""

    HKEY_MAP = {
        "HKLM": winreg.HKEY_LOCAL_MACHINE,
        "HKCU": winreg.HKEY_CURRENT_USER,
    }

    @classmethod
    def apply_dword(cls, root: str, subkey: str, value_name: str, new_value: int) -> Optional[int]:
        """Aplica um valor DWORD salvando o valor anterior para suportar Rollback atômico."""
        h_root = cls.HKEY_MAP.get(root)
        if not h_root:
            raise ValueError(f"Root HKEY inválida: {root}")

        previous_value = None
        try:
            with winreg.OpenKey(h_root, subkey, 0, winreg.KEY_READ) as key:
                val, val_type = winreg.QueryValueEx(key, value_name)
                if val_type == winreg.REG_DWORD:
                    previous_value = val
        except FileNotFoundError:
            previous_value = None

        with winreg.CreateKeyEx(h_root, subkey, 0, winreg.KEY_WRITE) as key:
            winreg.SetValueEx(key, value_name, 0, winreg.REG_DWORD, new_value)

        return previous_value

    @classmethod
    def rollback_dword(cls, root: str, subkey: str, value_name: str, original_value: Optional[int]):
        h_root = cls.HKEY_MAP.get(root)
        if not h_root:
            return

        if original_value is None:
            try:
                with winreg.OpenKey(h_root, subkey, 0, winreg.KEY_WRITE) as key:
                    winreg.DeleteValue(key, value_name)
            except FileNotFoundError:
                pass
        else:
            with winreg.CreateKeyEx(h_root, subkey, 0, winreg.KEY_WRITE) as key:
                winreg.SetValueEx(key, value_name, 0, winreg.REG_DWORD, original_value)


if __name__ == "__main__":
    print("[Vantage OS Native Core] Testando telemetria e permissões...")
    print(f"Is Admin: {SecurityManager.is_admin()}")
    telemetry = HardwareTelemetry()
    print(f"RAM Metrics: {telemetry.get_memory_metrics()}")
    print(f"CPU Load: {telemetry.get_cpu_load()}%")
