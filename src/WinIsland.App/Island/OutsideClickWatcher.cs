// OutsideClickWatcher.cs: closes the expanded island when a mouse button is pressed anywhere
// outside it, for example in the game (Diva's request, 31 Oct 2026).
// The island never has focus, so clicks elsewhere never reach it. Instead, only while the island
// is expanded, this checks the mouse buttons 20 times a second. It reads the button state only:
// no mouse hook (PRD section 7: anti-cheat could flag one), and the click still reaches the game.
// While the island is closed, nothing runs.

using System.Windows;
using System.Windows.Threading;

using WinIsland.Core.Island;
using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

internal sealed class OutsideClickWatcher : IDisposable
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(50);

    private readonly IslandStateMachine _island;
    private readonly FrameworkElement _pill;
    private readonly DispatcherTimer _timer;
    private bool _wasButtonDown;

    public OutsideClickWatcher(IslandStateMachine island, FrameworkElement pill)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(pill);
        _island = island;
        _pill = pill;

        _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = CheckInterval };
        _timer.Tick += (_, _) => Check();
        _island.Changed += (_, _) => FollowIsland();
    }

    public void Dispose()
    {
        _timer.Stop();
    }

    private void FollowIsland()
    {
        bool isExpanded = _island.View == IslandView.Expanded;
        if (isExpanded && !_timer.IsEnabled)
        {
            // The click that opened the island may still be held down; only a new press counts.
            _wasButtonDown = PointerState.IsAnyButtonDown();
            _timer.Start();
        }
        else if (!isExpanded && _timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    private void Check()
    {
        bool isDown = PointerState.IsAnyButtonDown();
        bool isNewPress = isDown && !_wasButtonDown;
        _wasButtonDown = isDown;

        if (isNewPress && !IsPointerOverPill())
        {
            _island.Collapse(DateTimeOffset.Now);
        }
    }

    private bool IsPointerOverPill()
    {
        if (PointerState.Position() is not (int x, int y) || PresentationSource.FromVisual(_pill) is null)
        {
            // Without a position, assume the click was on the island rather than closing it by mistake.
            return true;
        }

        Point inside = _pill.PointFromScreen(new Point(x, y));
        return inside.X >= 0 && inside.Y >= 0 && inside.X <= _pill.ActualWidth && inside.Y <= _pill.ActualHeight;
    }
}
