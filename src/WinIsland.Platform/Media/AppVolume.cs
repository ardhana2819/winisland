// AppVolume.cs: reads and sets the volume of one app's sound, as the Windows Volume Mixer does
// (PRD FR-39). Windows' own volume and every other app (the game) stay as they are.
// It goes through the Windows audio session API (CoreAudio.cs) on the default playback device:
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
            foreach (IAudioSessionControl2 session in CoreAudio.PlaybackSessions())
            {
                if (session.GetProcessId(out uint processId) == 0
                    && BelongsTo(mediaAppId, processId)
                    && session is ISimpleAudioVolume volume)
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

    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ErrorInsufficientBuffer = 122;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint processId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr handle);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetPackageFamilyName(IntPtr process, ref uint length, [Out] char[]? name);
}
