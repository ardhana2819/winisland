// CoreAudio.cs: the small part of the Windows audio session API (Core Audio) that WinIsland uses,
// in one place for the music app's volume (AppVolume.cs, PRD FR-39) and the microphone dot
// (MicrophoneUse.cs, FR-40). Each sound an app plays or records is an audio session on a device;
// a session tells which process it belongs to, whether it is running, and (for playback) its
// volume. Only these facts are read; nothing here listens to or records any sound.
// Interface ids are copied from the Windows SDK headers mmdeviceapi.h, audiopolicy.h and
// audioclient.h (checked on 3 Nov 2026 after a wrong id silently disabled the volume slider).

using System.Runtime.InteropServices;

namespace WinIsland.Platform.Media;

internal static class CoreAudio
{
    public const int Playback = 0;
    public const int Recording = 1;
    public const int ActiveState = 1;

    private const int RoleMultimedia = 1;
    private const int DeviceStateActive = 1;
    private const int ClsctxAll = 0x17;

    /// <summary>The audio sessions on the default playback device.</summary>
    public static IEnumerable<IAudioSessionControl2> PlaybackSessions()
    {
        IMMDeviceEnumerator devices = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        Marshal.ThrowExceptionForHR(devices.GetDefaultAudioEndpoint(Playback, RoleMultimedia, out IMMDevice speaker));
        return SessionsOn(speaker);
    }

    /// <summary>The audio sessions on every microphone that is plugged in and on.</summary>
    public static IEnumerable<IAudioSessionControl2> RecordingSessions()
    {
        IMMDeviceEnumerator devices = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        Marshal.ThrowExceptionForHR(devices.EnumAudioEndpoints(Recording, DeviceStateActive, out IMMDeviceCollection microphones));
        Marshal.ThrowExceptionForHR(microphones.GetCount(out int count));
        for (int index = 0; index < count; index++)
        {
            Marshal.ThrowExceptionForHR(microphones.Item(index, out IMMDevice microphone));
            foreach (IAudioSessionControl2 session in SessionsOn(microphone))
            {
                yield return session;
            }
        }
    }

    private static IEnumerable<IAudioSessionControl2> SessionsOn(IMMDevice device)
    {
        Guid managerId = typeof(IAudioSessionManager2).GUID;
        Marshal.ThrowExceptionForHR(device.Activate(ref managerId, ClsctxAll, IntPtr.Zero, out object managerObject));
        IAudioSessionManager2 manager = (IAudioSessionManager2)managerObject;
        Marshal.ThrowExceptionForHR(manager.GetSessionEnumerator(out IAudioSessionEnumerator sessions));
        Marshal.ThrowExceptionForHR(sessions.GetCount(out int count));

        List<IAudioSessionControl2> found = [];
        for (int index = 0; index < count; index++)
        {
            Marshal.ThrowExceptionForHR(sessions.GetSession(index, out IAudioSessionControl control));
            if (control is IAudioSessionControl2 details)
            {
                found.Add(details);
            }
        }

        return found;
    }

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
        int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);

        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
    }

    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceCollection
    {
        int GetCount(out int count);

        int Item(int index, out IMMDevice device);
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
}

/// <summary>One audio session: its state and its process (audiopolicy.h, in the SDK's order).</summary>
[ComImport]
[Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionControl2
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

/// <summary>A playback session's volume (audioclient.h).</summary>
[ComImport]
[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISimpleAudioVolume
{
    int SetMasterVolume(float level, ref Guid eventContext);

    int GetMasterVolume(out float level);
}
