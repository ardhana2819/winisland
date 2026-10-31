// TopmostGuard.cs: keeps the island on top (PRD FR-1, FR-3).
// Diva found that after closing and reopening Spotify, WhatsApp or Discord, the island could fall
// behind maximized Chrome and Spotify windows until WinIsland was restarted (31 Oct 2026). The
// exact Windows trigger could not be reproduced on demand, so every 2 seconds this checks that
// the island window still has its on-top, no-activate and tool-window styles and puts back
// whatever is missing. Each repair is logged with the missing style names (no other data), so
// the log shows whether and when it happens.
// It does not push the island above other always-on-top windows (for example Snipping Tool's
// toolbar); it only repairs the island's own styles. The check is one Windows call: cheap.

using System.Diagnostics;
using System.Windows.Threading;

using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

internal sealed class TopmostGuard : IDisposable
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(2);

    private readonly IntPtr _windowHandle;
    private readonly DispatcherTimer _timer;

    public TopmostGuard(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = CheckInterval };
        _timer.Tick += (_, _) => CheckNow();
        _timer.Start();
    }

    /// <summary>Checks and repairs at once, for example right after the display changed.</summary>
    public void CheckNow()
    {
        IReadOnlyList<string> repaired = NonActivatingWindow.RepairIfNeeded(_windowHandle);
        if (repaired.Count > 0)
        {
            Trace.TraceWarning("Island window had lost {0}; restored", string.Join(", ", repaired));
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
