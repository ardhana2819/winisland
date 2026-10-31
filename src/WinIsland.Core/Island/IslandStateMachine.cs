// IslandStateMachine.cs: decides what the island should be doing, without drawing anything.
// It holds the rules from the PRD so they can be unit tested:
//   FR-2  expanding only works while music is present.
//   FR-5  hotkey 1 toggles expanded, hotkey 2 hides or shows.
//   FR-6  pause for 1 hour, then return on its own; Resume ends it early.
//   FR-10 music can appear, pause, change and disappear at any time.
//   12    paused wins over everything.
//   13    paused music shows for 5 seconds, then the clock; while the island is expanded the
//         music stays, and the 5 seconds start when it closes.
// Time is passed in by the caller instead of read from the clock, so tests do not have to wait.
// NextTimeCheckAt tells the caller when the next time-based change is due.

using WinIsland.Core.Music;

namespace WinIsland.Core.Island;

public sealed class IslandStateMachine
{
    public static readonly TimeSpan PauseDuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan PausedMusicDuration = TimeSpan.FromSeconds(5);

    private bool _isExpanded;
    private DateTimeOffset? _pausedMusicEndsAt;

    /// <summary>Raised after any change that affects what the island shows.</summary>
    public event EventHandler? Changed;

    public bool IsHiddenByUser { get; private set; }

    public DateTimeOffset? PausedUntil { get; private set; }

    /// <summary>The latest report from the music player, or null when there is no music session.</summary>
    public NowPlaying? Music { get; private set; }

    /// <summary>True while the island shows music: playing, recently paused, or expanded.</summary>
    public bool IsMusicPresent { get; private set; }

    public bool IsPaused => PausedUntil is not null;

    public bool IsVisible => !IsHiddenByUser && !IsPaused;

    /// <summary>The conflict banner text while it is showing, otherwise null.</summary>
    public string? HotkeyConflictText { get; private set; }

    public IslandView View
    {
        get
        {
            if (!IsVisible)
            {
                return IslandView.Compact;
            }

            if (HotkeyConflictText is not null)
            {
                return IslandView.HotkeyConflict;
            }

            if (_isExpanded)
            {
                return IslandView.Expanded;
            }

            return IsMusicPresent ? IslandView.Music : IslandView.Compact;
        }
    }

    /// <summary>When the next time-based change is due (end of pause, or paused music going back to the clock).</summary>
    public DateTimeOffset? NextTimeCheckAt
    {
        get
        {
            if (PausedUntil is DateTimeOffset pauseEnd && _pausedMusicEndsAt is DateTimeOffset musicEnd)
            {
                return pauseEnd < musicEnd ? pauseEnd : musicEnd;
            }

            return PausedUntil ?? _pausedMusicEndsAt;
        }
    }

    /// <summary>Hotkey 1 or a click on the compact island. Does nothing without music (FR-2).</summary>
    public void ToggleExpanded(DateTimeOffset now)
    {
        if (_isExpanded)
        {
            Collapse(now);
            return;
        }

        if (!IsVisible || !IsMusicPresent)
        {
            return;
        }

        _isExpanded = true;
        // While expanded, paused music stays; its 5 seconds start again when the island closes.
        _pausedMusicEndsAt = null;
        RaiseChanged();
    }

    /// <summary>Closes the expanded island: click outside the buttons, or 2 s after the pointer leaves (FR-2).</summary>
    public void Collapse(DateTimeOffset now)
    {
        if (!_isExpanded)
        {
            return;
        }

        CloseExpanded(now);
        RaiseChanged();
    }

    /// <summary>Hotkey 2 or the tray's Hide island item (FR-5, FR-6).</summary>
    public void ToggleHidden(DateTimeOffset now)
    {
        IsHiddenByUser = !IsHiddenByUser;
        // A hidden island cannot be pointed at, so it would close anyway; close it now.
        CloseExpanded(now);
        RaiseChanged();
    }

    /// <summary>Tray: Pause for 1 hour (FR-6).</summary>
    public void Pause(DateTimeOffset now)
    {
        PausedUntil = now + PauseDuration;
        CloseExpanded(now);
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

    /// <summary>Called by the app at NextTimeCheckAt (and once a minute as a backup).</summary>
    public void UpdateTime(DateTimeOffset now)
    {
        if (PausedUntil is DateTimeOffset pauseEnd && now >= pauseEnd)
        {
            Resume();
        }

        if (_pausedMusicEndsAt is DateTimeOffset musicEnd && now >= musicEnd)
        {
            _pausedMusicEndsAt = null;
            IsMusicPresent = false;
            RaiseChanged();
        }
    }

    /// <summary>The latest report from the music player (FR-7, FR-10). Null means no music session.</summary>
    public void UpdateMusic(NowPlaying? music, DateTimeOffset now)
    {
        NowPlaying? previous = Music;
        Music = music;

        if (music is null)
        {
            // Spotify closed or stopped publishing: back to the clock, even when expanded.
            IsMusicPresent = false;
            _pausedMusicEndsAt = null;
            _isExpanded = false;
        }
        else if (music.IsPlaying)
        {
            IsMusicPresent = true;
            _pausedMusicEndsAt = null;
        }
        else if (previous is null || previous.IsPlaying)
        {
            // Music just stopped playing (or showed up paused): show it for a while, then the clock.
            IsMusicPresent = true;
            _pausedMusicEndsAt = _isExpanded ? null : now + PausedMusicDuration;
        }

        if (!Equals(previous, music))
        {
            RaiseChanged();
        }
    }

    /// <summary>Shows the hotkey conflict banner (FR-5). The app hides it again after 10 seconds.</summary>
    public void ShowHotkeyConflict(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("The conflict banner needs a message.", nameof(message));
        }

        HotkeyConflictText = message;
        RaiseChanged();
    }

    public void DismissHotkeyConflict()
    {
        if (HotkeyConflictText is null)
        {
            return;
        }

        HotkeyConflictText = null;
        RaiseChanged();
    }

    private void CloseExpanded(DateTimeOffset now)
    {
        if (!_isExpanded)
        {
            return;
        }

        _isExpanded = false;
        if (IsMusicPresent && Music is { IsPlaying: false })
        {
            _pausedMusicEndsAt = now + PausedMusicDuration;
        }
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
