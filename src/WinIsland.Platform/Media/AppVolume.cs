// AppVolume.cs: reads and sets the volume of one app's sound, as the Windows Volume Mixer does
// (PRD FR-39). Windows' own volume and every other app (the game) stay as they are.
// It goes through the Windows audio session API (Core Audio) on the default playback device:
// each sound an app plays is an audio session with its own volume. AudioAppMatch (Core) decides
// which sessions belong to the music app, from the process name and, for Store apps, the package.
// Only the volume is read or changed; nothing here listens to or records any sound.

using System.Diagnostics;
using System.Runtime.InteropServices;

using WinIsland.Core.Music;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Media;

public static partial class AppVolume
{
    /// <summary>The music app's volume, 0 to 1, or null when it has no sound open with Windows.</summary>
    public static float? Get(string mediaAppId)
    {
        float? volume = null;
        ForEachSessionOf(mediaAppId, session =>
        {
            Marshal.ThrowExceptionForHR(session.GetMasterVolume(out float level));
            volume ??= level;
        });
        return volume;
    }

    /// <summary>Sets the music app's volume, 0 to 1. Returns false when it has no sound open with Windows.</summary>
    public static bool Set(string mediaAppId, float volume)
    {
        float level = Math.Clamp(volume, 0f, 1f);
        Guid noContext = Guid.Empty;
        bool found = false;
        ForEachSessionOf(mediaAppId, session =>
        {
            Marshal.ThrowExceptionForHR(session.SetMasterVolume(level, ref noContext));
            found = true;
        });
        return found;
    }

    private static void ForEachSessionOf(string mediaAppId, Action<ISimpleAudioVolume> act)
    {
        if (string.IsNullOrWhiteSpace(mediaAppId))
        {
            return;
        }

        try
        {
            IMMDeviceEnumerator devices = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            Marshal.ThrowExceptionForHR(devices.GetDefaultAudioEndpoint(DataFlowRender, RoleMultimedia, out IMMDevice speaker));
            Guid managerId = typeof(IAudioSessionManager2).GUID;
            Marshal.ThrowExceptionForHR(speaker.Activate(ref managerId, ClsctxAll, IntPtr.Zero, out object managerObject));
            IAudioSessionManager2 manager = (IAudioSessionManager2)managerObject;
            Marshal.ThrowExceptionForHR(manager.GetSessionEnumerator(out IAudioSessionEnumerator sessions));
            Marshal.ThrowExceptionForHR(sessions.GetCount(out int count));

            for (int index = 0; index < count; index++)
            {
                Marshal.ThrowExceptionForHR(sessions.GetSession(index, out IAudioSessionControl control));
                if (control is IAudioSessionControl2 details
                    && details.GetProcessId(out uint processId) == 0
                    && BelongsTo(mediaAppId, processId)
                    && control is ISimpleAudioVolume volume)
                {
                    act(volume);
                }
            }
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Reaching the music app's volume failed", error);
        }
    }

    private static bool BelongsTo(string mediaAppId, uint processId)
    {
        if (processId == 0)
        {
            return false;
        }

        try
        {
            using Process process = Process.GetProcessById((int)processId);
            return AudioAppMatch.IsMusicApp(mediaAppId, process.ProcessName, PackageFamilyOf(processId));
        }
        catch (ArgumentException)
        {
            // The process ended between listing the sessions and asking about it.
            return false;
        }
    }

    // Store apps only; desktop apps have no package and give null.
    private static string? PackageFamilyOf(uint processId)
    {
        IntPtr process = OpenProcess(ProcessQueryLimitedInformation, false, processId);
        if (process == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            uint length = 0;
            if (GetPackageFamilyName(process, ref length, null) != ErrorInsufficientBuffer)
            {
                return null;
            }

            char[] name = new char[length];
            return GetPackageFamilyName(process, ref length, name) == 0 ? new string(name, 0, (int)length - 1) : null;
        }
        finally
        {
            CloseHandle(process);
        }
    }

    private const int DataFlowRender = 0;
    private const int RoleMultimedia = 1;
    private const int ClsctxAll = 0x17;
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ErrorInsufficientBuffer = 122;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint processId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr handle);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetPackageFamilyName(IntPtr process, ref uint length, [Out] char[]? name);

    // Core Audio interfaces (mmdeviceapi.h, audiopolicy.h), only the methods used here, in order.
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumerator
    {
    }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices);

        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        int Activate(ref Guid interfaceId, int classContext, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object instance);
    }

    [ComImport]
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionManager2
    {
        int GetAudioSessionControl(IntPtr sessionId, int flags, out IntPtr control);

        int GetSimpleAudioVolume(IntPtr sessionId, int flags, out IntPtr volume);

        int GetSessionEnumerator(out IAudioSessionEnumerator sessions);
    }

    [ComImport]
    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionEnumerator
    {
        int GetCount(out int count);

        int GetSession(int index, out IAudioSessionControl session);
    }

    [ComImport]
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionControl
    {
    }

    [ComImport]
    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionControl2
    {
        int GetState(out int state);

        int GetDisplayName(out IntPtr name);

        int SetDisplayName(IntPtr name, IntPtr context);

        int GetIconPath(out IntPtr path);

        int SetIconPath(IntPtr path, IntPtr context);

        int GetGroupingParam(out Guid group);

        int SetGroupingParam(IntPtr group, IntPtr context);

        int RegisterAudioSessionNotification(IntPtr client);

        int UnregisterAudioSessionNotification(IntPtr client);

        int GetSessionIdentifier(out IntPtr id);

        int GetSessionInstanceIdentifier(out IntPtr id);

        int GetProcessId(out uint processId);
    }

    [ComImport]
    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ISimpleAudioVolume
    {
        int SetMasterVolume(float level, ref Guid eventContext);

        int GetMasterVolume(out float level);
    }
}
