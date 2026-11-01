// PointerLeaveCollapser.cs: closes the expanded island about 2 seconds after the mouse pointer
// leaves it (PRD FR-2, DESIGN_SPEC section 7). Coming back before then keeps it open.

using System.Windows;
using System.Windows.Threading;

using WinIsland.Core.Island;

namespace WinIsland.App.Island;

internal sealed class PointerLeaveCollapser : IDisposable
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(2);

    private readonly IslandStateMachine _island;
    private readonly DispatcherTimer _timer;

    public PointerLeaveCollapser(IslandStateMachine island, UIElement pill)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(pill);
        _island = island;

        _timer = new DispatcherTimer { Interval = Delay };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            _island.Collapse(DateTimeOffset.Now);
        };

        pill.MouseEnter += (_, _) => _timer.Stop();
        pill.MouseLeave += (_, _) =>
        {
            if (_island.View == IslandView.Expanded)
            {
                _timer.Start();
            }
        };
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
