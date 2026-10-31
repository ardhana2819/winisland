// PlatformLog.cs: records Windows API problems so they are never swallowed silently.
// Only the kind of problem and its error code are written, never music titles or message text
// (PRD FR-18, section 14). It writes to the .NET trace, which a debugger or a later log file can
// listen to; nothing is written to disk by this class.

using System.Diagnostics;

namespace WinIsland.Platform.Diagnostics;

internal static class PlatformLog
{
    public static void Problem(string what, Exception error)
    {
        Trace.TraceWarning("{0}: {1} (HRESULT 0x{2:X8})", what, error.GetType().Name, error.HResult);
    }
}
