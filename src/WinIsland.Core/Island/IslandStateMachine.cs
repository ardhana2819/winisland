// IslandStateMachine.cs: decides what the island should be doing, without drawing anything.
// It holds the rules from the PRD so they can be unit tested:
//   FR-2  expanding only works while music is present.
//   FR-5  hotkey 1 toggles expanded, hotkey 2 hides or shows.
//   FR-6  pause for 1 hour, then return on its own; Resume ends it early.
//   12    paused wins over everything.
// Time is passed in by the caller instead of read from the clock, so tests do not have to wait.

namespace WinIsland.Core.Island;

public sealed class IslandStateMachine
{
    public static readonly TimeSpan PauseDuration = TimeSpan.FromHours(1);

    private bool _isExpanded;

    /// <summary>Raised after any change that affects what the island shows.</summary>
    public event EventHandler? Changed;

    public bool IsHiddenByUser { get; private set; }

    public DateTimeOffset? PausedUntil { get; private set; }

    public bool IsMusicPresent { get; private set; }

    public bool IsPaused => PausedUntil is not null;

    public bool IsVisible => !IsHiddenByUser && !IsPaused;

    public IslandView View => IsVisible && _isExpanded ? IslandView.Expanded : IslandView.Compact;

    /// <summary>Hotkey 1 or a click on the compact island. Does nothing without music (FR-2).</summary>
    public void ToggleExpanded()
    {
        if (_isExpanded)
        {
            Collapse();
            return;
        }

        if (!IsVisible || !IsMusicPresent)
        {
            return;
        }

        _isExpanded = true;
        RaiseChanged();
    }

    /// <summary>Closes the expanded island: click outside the buttons, or 2 s after the pointer leaves (FR-2).</summary>
    public void Collapse()
    {
        if (!_isExpanded)
        {
            return;
        }

        _isExpanded = false;
        RaiseChanged();
    }

    /// <summary>Hotkey 2 or the tray's Hide island item (FR-5, FR-6).</summary>
    public void ToggleHidden()
    {
        IsHiddenByUser = !IsHiddenByUser;
        // A hidden island cannot be pointed at, so it would close anyway; close it now.
        _isExpanded = false;
        RaiseChanged();
    }

    /// <summary>Tray: Pause for 1 hour (FR-6).</summary>
    public void Pause(DateTimeOffset now)
    {
        PausedUntil = now + PauseDuration;
        _isExpanded = false;
        RaiseChanged();
    }

    /// <summary>Tray: Resume, which ends the pause early (FR-6).</summary>
    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        PausedUntil = null;
        RaiseChanged();
    }

    /// <summary>Called regularly by the app so the pause ends on its own after an hour.</summary>
    public void UpdateTime(DateTimeOffset now)
    {
        if (PausedUntil is DateTimeOffset end && now >= end)
        {
            Resume();
        }
    }

    /// <summary>Set by the music feature (Phase 3). When music goes away, the expanded island closes.</summary>
    public void SetMusicPresent(bool isPresent)
    {
        if (IsMusicPresent == isPresent)
        {
            return;
        }

        IsMusicPresent = isPresent;
        if (!isPresent)
        {
            _isExpanded = false;
        }

        RaiseChanged();
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
