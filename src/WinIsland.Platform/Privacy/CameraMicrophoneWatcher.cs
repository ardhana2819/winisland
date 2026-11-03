// CameraMicrophoneWatcher.cs: tells whether any app uses the camera or the microphone right now
// (PRD FR-40), for the green and orange dots on the island.
// It reads Windows' own record of which apps use them, the same record behind the microphone
// symbol on the Windows 11 taskbar: under the current user's CapabilityAccessManager consent store,
// each app (and each desktop program under "NonPackaged") has a start and a stop time; an app uses
// the device while it has started and not stopped (PrivacyIndicators.IsInUse in Core).
// It never touches the camera or microphone, and it learns only "in use" or not. Instead of
// checking on a timer it lets Windows wake it when the record changes, so it costs nothing idle.
// Microsoft does not document this record [Unverified]; if it is missing, the dots never show.

using System.Runtime.InteropServices;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

using WinIsland.Core.Privacy;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Privacy;

public sealed partial class CameraMicrophoneWatcher : IDisposable
{
    private const string ConsentStore = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore";
    private const string Camera = "webcam";
    private const string Microphone = "microphone";
    private const string DesktopPrograms = "NonPackaged";
    private const int NotifyChangeName = 0x1;
    private const int NotifyChangeLastSet = 0x4;

    private readonly ManualResetEvent _stop = new(false);
    private readonly Thread _thread;
    private PrivacyIndicator _shown = PrivacyIndicator.None;

    public CameraMicrophoneWatcher()
    {
        _thread = new Thread(Watch) { IsBackground = true, Name = "Camera and microphone watcher" };
    }

    /// <summary>Raised on a background thread whenever the dot to show changes.</summary>
    public event EventHandler<PrivacyIndicator>? Changed;

    public PrivacyIndicator Current => _shown;

    public void Start()
    {
        _thread.Start();
    }

    public void Dispose()
    {
        _stop.Set();
        _thread.Join(TimeSpan.FromSeconds(2));
        _stop.Dispose();
    }

    private void Watch()
    {
        try
        {
            using RegistryKey? store = Registry.CurrentUser.OpenSubKey(ConsentStore);
            if (store is null)
            {
                return;
            }

            using AutoResetEvent recordChanged = new(false);
            while (true)
            {
                // Ask first, then read, so a change between the two is not missed.
                int result = RegNotifyChangeKeyValue(store.Handle, true, NotifyChangeName | NotifyChangeLastSet, recordChanged.SafeWaitHandle, true);
                if (result != 0)
                {
                    throw new System.ComponentModel.Win32Exception(result);
                }

                Report(PrivacyIndicators.For(IsInUse(store, Camera), IsInUse(store, Microphone)));
                if (WaitHandle.WaitAny([recordChanged, _stop]) == 1)
                {
                    return;
                }
            }
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Watching camera and microphone use failed", error);
        }
    }

    private void Report(PrivacyIndicator indicator)
    {
        if (indicator == _shown)
        {
            return;
        }

        _shown = indicator;
        Changed?.Invoke(this, indicator);
    }

    private static bool IsInUse(RegistryKey store, string device)
    {
        using RegistryKey? apps = store.OpenSubKey(device);
        if (apps is null)
        {
            return false;
        }

        using RegistryKey? programs = apps.OpenSubKey(DesktopPrograms);
        return AnyInUse(apps) || (programs is not null && AnyInUse(programs));
    }

    private static bool AnyInUse(RegistryKey parent)
    {
        foreach (string name in parent.GetSubKeyNames())
        {
            using RegistryKey? app = parent.OpenSubKey(name);
            if (app?.GetValue("LastUsedTimeStart") is long start
                && app.GetValue("LastUsedTimeStop") is long stop
                && PrivacyIndicators.IsInUse(start, stop))
            {
                return true;
            }
        }

        return false;
    }

    [LibraryImport("advapi32.dll")]
    private static partial int RegNotifyChangeKeyValue(SafeRegistryHandle key, [MarshalAs(UnmanagedType.Bool)] bool watchSubtree, int filter, SafeWaitHandle changed, [MarshalAs(UnmanagedType.Bool)] bool asynchronous);
}
