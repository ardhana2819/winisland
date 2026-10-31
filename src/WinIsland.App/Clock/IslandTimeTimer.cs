// IslandTimeTimer.cs: wakes the island exactly when a time-based change is due.
// Core says when that is (NextTimeCheckAt): the end of "Pause for 1 hour" (PRD FR-6), or paused
// music going back to the clock after 5 seconds (section 13). One timer covers both, and nothing
// runs while no change is due. The once-per-minute clock tick is a backup for when the PC slept
// through the due time.

using System.Windows.Threading;

using WinIsland.Core.Island;

namespace WinIsland.App.Clock;

internal sealed class IslandTimeTimer : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly DispatcherTimer _timer;

    public IslandTimeTimer(IslandStateMachine island)
    {
        ArgumentNullException.ThrowIfNull(island);
        _island = island;
        _timer = new DispatcherTimer(DispatcherPriority.Background);
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            _island.UpdateTime(DateTimeOffset.Now);
        };
        _island.Changed += (_, _) => FollowNextCheck();
    }

    public void Dispose()
    {
        _timer.Stop();
    }

    private void FollowNextCheck()
    {
        if (_island.NextTimeCheckAt is not DateTimeOffset due)
        {
            _timer.Stop();
            return;
        }

        TimeSpan remaining = due - DateTimeOffset.Now;
        _timer.Stop();
        _timer.Interval = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        _timer.Start();
    }
}
