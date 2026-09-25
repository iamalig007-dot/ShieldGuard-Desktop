using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace ShieldGuard.Desktop.Services;

public class StartupService
{
    private const string StartupKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ShieldGuard";

    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(StartupKeyPath);
            return key?.GetValue(AppName) != null;
        }
        catch { return false; }
    }

    public void EnableStartup()
    {
        try
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            using var key = Registry.LocalMachine.OpenSubKey(StartupKeyPath, writable: true);
            key?.SetValue(AppName, $"\"{exePath}\"");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not set startup: {ex.Message}");
        }
    }

    public void DisableStartup()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(StartupKeyPath, writable: true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
        catch { }
    }
}
