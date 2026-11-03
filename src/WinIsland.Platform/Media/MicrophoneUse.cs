// MicrophoneUse.cs: tells whether any other app records from a microphone right now (PRD FR-40),
// through the Windows audio session API (documented): every app that records has a running
// session on a microphone. This replaced Windows' consent record for the microphone, because on
// Diva's PC (Windows 11 24H2) that record lists no desktop apps at all, so Discord voice never
// showed (3 Nov 2026). WinIsland only reads whether a session runs; it never opens a microphone.

using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Media;

public static class MicrophoneUse
{
    public static bool IsAnyAppRecording()
    {
        try
        {
            uint ownProcess = (uint)Environment.ProcessId;
            foreach (IAudioSessionControl2 session in CoreAudio.RecordingSessions())
            {
                bool isRunning = session.GetState(out int state) == 0 && state == CoreAudio.ActiveState;
                bool isOtherApp = session.GetProcessId(out uint processId) == 0 && processId != 0 && processId != ownProcess;
                if (isRunning && isOtherApp)
                {
                    return true;
                }
            }
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Checking microphone use failed", error);
        }

        return false;
    }
}
