// StartupRegistration.cs: makes WinIsland start with Windows, or stops it (PRD FR-20, AC-8).
// Uses the standard per-user "Run" list in the registry (HKEY_CURRENT_USER), the same place
// Windows Settings > Apps > Startup reads. No admin rights, nothing for other users. WinIsland
// has no main window, so starting it this way already means "minimized to the tray".

using Microsoft.Win32;

using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Startup;

public static class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WinIsland";

    /// <summary>Starts WinIsland with Windows from this exe, or stops that. Returns false when Windows refused.</summary>
    public static bool Apply(bool launchAtStartup, string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        try
        {
            using RegistryKey runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (launchAtStartup)
            {
                // Quoted, because the folder name can contain spaces ("Claude Project").
                string command = $"\"{executablePath}\"";
                if (!string.Equals(runKey.GetValue(ValueName) as string, command, StringComparison.OrdinalIgnoreCase))
                {
                    runKey.SetValue(ValueName, command);
                }
            }
            else if (runKey.GetValue(ValueName) is not null)
            {
                runKey.DeleteValue(ValueName);
            }

            return true;
        }
        catch (Exception error) when (error is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            PlatformLog.Problem("Changing the start-with-Windows setting failed", error);
            return false;
        }
    }
}
