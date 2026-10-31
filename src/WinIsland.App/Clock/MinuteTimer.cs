// MinuteTimer.cs: tells the island when the minute changes, so the clock can update.
// The clock shows only hours and minutes, so waking up once per minute (just after the minute
// starts) shows exactly the same thing as checking every second, with far less work (PRD G4).
// It also reacts right away when the Windows time changes or the PC wakes from sleep.

using System.Windows.Threading;

using Microsoft.Win32;

namespace WinIsland.App.Clock;

internal sealed class MinuteTimer : IDisposable
{
    // Fire slightly after the minute starts, so the new minute is already showing.
    private static readonly TimeSpan SafetyMargin = TimeSpan.FromMilliseconds(50);

    private readonly DispatcherTimer _timer;

    public MinuteTimer()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background);
        _timer.Tick += (_, _) => RaiseAndReschedule();
        SystemEvents.TimeChanged += OnSystemTimeChanged;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    /// <summary>Raised with the current local time: once at start, then at every new minute.</summary>
    public event EventHandler<DateTime>? MinuteChanged;

    public void Start()
    {
        RaiseAndReschedule();
    }

    public void Dispose()
    {
        _timer.Stop();
        // SystemEvents keeps static references, so unsubscribe to let this object be freed.
        SystemEvents.TimeChanged -= OnSystemTimeChanged;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }

    private void RaiseAndReschedule()
    {
        _timer.Stop();
        DateTime now = DateTime.Now;
        MinuteChanged?.Invoke(this, now);

        DateTime nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind).AddMinutes(1);
        _timer.Interval = (nextMinute - now) + SafetyMargin;
        _timer.Start();
    }

    private void OnSystemTimeChanged(object? sender, EventArgs e)
    {
        _timer.Dispatcher.BeginInvoke(RaiseAndReschedule);
    }

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
        {
            _timer.Dispatcher.BeginInvoke(RaiseAndReschedule);
        }
    }
}
