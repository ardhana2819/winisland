// PauseEndTimer.cs: ends "Pause for 1 hour" on time (PRD FR-6).
// While the island is paused, it waits until the exact end time and then tells the island what
// time it is, so the island returns on its own. The once-per-minute clock tick also does this,
// which covers the case where the PC was asleep when the hour ended.

using System.Windows.Threading;

using WinIsland.Core.Island;

namespace WinIsland.App.Clock;

internal sealed class PauseEndTimer : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly DispatcherTimer _timer;

    public PauseEndTimer(IslandStateMachine island)
    {
        ArgumentNullException.ThrowIfNull(island);
        _island = island;
        _timer = new DispatcherTimer(DispatcherPriority.Background);
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            _island.UpdateTime(DateTimeOffset.Now);
        };
        _island.Changed += (_, _) => FollowPause();
    }

    public void Dispose()
    {
        _timer.Stop();
    }

    private void FollowPause()
    {
        if (_island.PausedUntil is not DateTimeOffset end)
        {
            _timer.Stop();
            return;
        }

        TimeSpan remaining = end - DateTimeOffset.Now;
        _timer.Interval = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        _timer.Start();
    }
}
