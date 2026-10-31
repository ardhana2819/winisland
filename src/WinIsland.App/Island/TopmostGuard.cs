// TopmostGuard.cs: keeps the island on top (PRD FR-1, FR-3).
// Diva found that after WhatsApp desktop starts, the island fell behind maximized Chrome and
// Spotify windows until WinIsland was restarted (31 Oct 2026). Checked live: the island kept its
// on-top style, but Windows had moved it below ordinary windows. Every 2 seconds this checks the
// island's on-top, no-activate and tool-window styles and its place among the on-top windows,
// and puts back whatever is wrong. Each repair is logged with what was wrong (no other data),
// so the log shows whether and when it happens.
// It does not push the island above other always-on-top windows (for example Snipping Tool's
// toolbar); it only acts when an ordinary window sits above the island. The check reads a few
// window styles: cheap.

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
