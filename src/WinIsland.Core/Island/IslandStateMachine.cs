// IslandStateMachine.cs: decides what the island should be doing, without drawing anything.
// It holds the rules from the PRD so they can be unit tested:
//   FR-2  the click and hotkey 1 always expand, also from the clock (v1.1); a banner replaces the
//         expanded island.
//   FR-23 expanding after paused music went back to the clock shows the paused song again.
//   FR-24 with no music session the expanded island shows the empty view, and Spotify closing
//         while it is open switches it to that view instead of closing it.
//   FR-5  hotkey 1 toggles expanded, hotkey 2 hides or shows, hotkey 3 hides banners.
//   FR-6  pause for 1 hour, then return on its own; Resume ends it early; notifications that
//         arrive while paused are dropped.
//   FR-6a while hidden, notifications are still processed.
//   FR-10 music can appear, pause, change and disappear at any time.
//   FR-14 only enabled apps show.
//   FR-15 banners, +N; tapping a banner opens the notifications page (v1.1); FR-16 banner timer
//         and dismiss on read.
//   FR-21 the expanded island has two pages, music and notifications; it opens on music.
//   FR-22 on the notifications page a new notification goes straight onto the page, no banner;
//         hotkey 3 closes the island there.
//   12    paused wins over everything.
//   13    paused music shows for 5 seconds, then the clock; while the island is expanded the
//         music stays, and the 5 seconds start when it closes.
// Time is passed in by the caller instead of read from the clock, so tests do not have to wait.
// NextTimeCheckAt tells the caller when the next time-based change is due.

using WinIsland.Core.Music;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Island;

public sealed class IslandStateMachine
{
    public static readonly TimeSpan PauseDuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan PausedMusicDuration = TimeSpan.FromSeconds(5);

    private readonly NotificationFeed _notifications = new();
    private IslandOverlay _overlay = IslandOverlay.None;
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

    /// <summary>The page of the expanded island (FR-21). Only meaningful while View is Expanded.</summary>
    public ExpandedPage Page { get; private set; } = ExpandedPage.Music;

    public bool IsVisible => !IsHiddenByUser && !IsPaused;

    /// <summary>The conflict banner text while it is showing, otherwise null.</summary>
    public string? HotkeyConflictText { get; private set; }

    /// <summary>True while the "Allow notification access" prompt is showing (FR-12).</summary>
    public bool IsPermissionPromptShown { get; private set; }

    public NotificationSettings NotificationSettings { get; private set; } = NotificationSettings.Defaults;

    /// <summary>The last 3 notifications, newest first (FR-15).</summary>
    public IReadOnlyList<IslandNotification> RecentNotifications => _notifications.Recent;

    /// <summary>Banners still waiting, newest first; the first one is on screen, the rest make the +N badge.</summary>
    public IReadOnlyList<IslandNotification> WaitingBanners => _notifications.Waiting;

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

            if (IsPermissionPromptShown)
            {
                return IslandView.PermissionPrompt;
            }

            return _overlay switch
            {
                IslandOverlay.Expanded => IslandView.Expanded,
                IslandOverlay.Banner => IslandView.Notification,
                _ => IsMusicPresent ? IslandView.Music : IslandView.Compact,
            };
        }
    }

    /// <summary>When the next time-based change is due: pause end, paused music, banner timer, or a read check.</summary>
    public DateTimeOffset? NextTimeCheckAt
    {
        get
        {
            DateTimeOffset? earliest = null;
            foreach (DateTimeOffset? due in new[] { PausedUntil, _pausedMusicEndsAt, _notifications.NextTimeCheckAt })
            {
                if (due is not null && (earliest is null || due < earliest))
                {
                    earliest = due;
                }
            }

            return earliest;
        }
    }

    /// <summary>
    /// Hotkey 1 or a click on the compact island, clock or music (FR-2). Opens the music view:
    /// the playing or paused song (FR-23), or the empty view without a music session (FR-24).
    /// </summary>
    public void ToggleExpanded(DateTimeOffset now)
    {
        if (_overlay == IslandOverlay.Expanded)
        {
            Collapse(now);
            return;
        }

        if (!IsVisible)
        {
            return;
        }

        OpenExpanded(ExpandedPage.Music);
        RaiseChanged();
    }

    /// <summary>Scroll, Alt+Up or Alt+Down, or a page dot on the open island (FR-21).</summary>
    public void ShowPage(ExpandedPage page)
    {
        if (_overlay != IslandOverlay.Expanded || Page == page)
        {
            return;
        }

        Page = page;
        RaiseChanged();
    }

    /// <summary>
    /// Closes the expanded island: click outside, or 2 s after the pointer leaves (FR-2).
    /// Banners close on their own timer instead.
    /// </summary>
    public void Collapse(DateTimeOffset now)
    {
        if (_overlay != IslandOverlay.Expanded)
        {
            return;
        }

        CloseOverlay(now);
        RaiseChanged();
    }

    /// <summary>Hotkey 2 or the tray's Hide island item (FR-5, FR-6). Notifications keep coming in (FR-6a).</summary>
    public void ToggleHidden(DateTimeOffset now)
    {
        IsHiddenByUser = !IsHiddenByUser;
        // A hidden island cannot be pointed at, so an open island would close anyway.
        if (_overlay == IslandOverlay.Expanded)
        {
            CloseOverlay(now);
        }

        RaiseChanged();
    }

    /// <summary>Tray: Pause for 1 hour (FR-6). Waiting banners go away too.</summary>
    public void Pause(DateTimeOffset now)
    {
        PausedUntil = now + PauseDuration;
        _notifications.HideAllBanners();
        CloseOverlay(now);
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

        if (_notifications.UpdateTime(now))
        {
            CloseNotificationOverlayIfEmpty(now);
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
            // Spotify closed or stopped publishing: back to the clock. An open island stays open
            // and shows the empty view instead (FR-24).
            IsMusicPresent = false;
            _pausedMusicEndsAt = null;
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
            _pausedMusicEndsAt = _overlay == IslandOverlay.Expanded ? null : now + PausedMusicDuration;
        }

        if (!Equals(previous, music))
        {
            RaiseChanged();
        }
    }

    /// <summary>A new Windows notification (FR-13). Dropped while paused (FR-6) or when its app is off (FR-14).</summary>
    public void AddNotification(IslandNotification notification, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (IsPaused || !NotificationSettings.IsShown(notification.Source))
        {
            return;
        }

        if (!_notifications.Add(notification, now, NotificationSettings.BannerDuration))
        {
            return;
        }

        if (IsVisible && _overlay == IslandOverlay.Expanded && Page == ExpandedPage.Notifications)
        {
            // Already looking at the notifications: it goes to the top of the page, no banner (FR-2, FR-22).
            _notifications.HideAllBanners();
            RaiseChanged();
            return;
        }

        // The banner replaces whatever was open, including the expanded island (FR-2).
        CloseOverlay(now);
        _overlay = IslandOverlay.Banner;
        RaiseChanged();
    }

    /// <summary>Windows removed a notification: read in the app, or replaced by a newer message (decided 1 s later).</summary>
    public void RemoveNotification(uint windowsId, DateTimeOffset now)
    {
        _notifications.MarkRemoved(windowsId, now);
        // Nothing visible changes yet, but the read check is now due: let the app reschedule.
        RaiseChanged();
    }

    /// <summary>
    /// Tap on the banner: opens the expanded island on the notifications page and stops the banner
    /// timer (FR-15, FR-16, FR-21).
    /// </summary>
    public void OpenNotificationsPage()
    {
        if (_overlay != IslandOverlay.Banner)
        {
            return;
        }

        _notifications.HideAllBanners();
        OpenExpanded(ExpandedPage.Notifications);
        RaiseChanged();
    }

    /// <summary>Swipe on the banner: hides the current one and shows the next waiting one (FR-15).</summary>
    public void SwipeBanner(DateTimeOffset now)
    {
        if (_overlay != IslandOverlay.Banner)
        {
            return;
        }

        _notifications.HideCurrentBanner(now, NotificationSettings.BannerDuration);
        CloseNotificationOverlayIfEmpty(now);
        RaiseChanged();
    }

    /// <summary>
    /// Hotkey 3: hides all waiting banners, and closes the island when it is open on the
    /// notifications page. Nothing is deleted (FR-15, FR-22).
    /// </summary>
    public void HideNotifications(DateTimeOffset now)
    {
        _notifications.HideAllBanners();
        bool onNotificationsPage = _overlay == IslandOverlay.Expanded && Page == ExpandedPage.Notifications;
        if (_overlay == IslandOverlay.Banner || onNotificationsPage)
        {
            CloseOverlay(now);
        }

        RaiseChanged();
    }

    /// <summary>The Settings window (Phase 5) changes these; defaults until then.</summary>
    public void UpdateNotificationSettings(NotificationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        NotificationSettings = settings;
        RaiseChanged();
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

    /// <summary>
    /// Shows "Allow notification access" once, when Windows did not give access (FR-12, section 13).
    /// Clock and music keep working; the app hides the prompt again after a short while.
    /// </summary>
    public void ShowPermissionPrompt()
    {
        if (IsPermissionPromptShown)
        {
            return;
        }

        IsPermissionPromptShown = true;
        RaiseChanged();
    }

    public void DismissPermissionPrompt()
    {
        if (!IsPermissionPromptShown)
        {
            return;
        }

        IsPermissionPromptShown = false;
        RaiseChanged();
    }

    // An empty notifications page stays open and says "No notifications" (FR-22); only a banner
    // with nothing left to show closes.
    private void CloseNotificationOverlayIfEmpty(DateTimeOffset now)
    {
        if (_overlay == IslandOverlay.Banner && _notifications.CurrentBanner is null)
        {
            CloseOverlay(now);
        }
    }

    private void OpenExpanded(ExpandedPage page)
    {
        _overlay = IslandOverlay.Expanded;
        Page = page;
        // A paused song counts as present again, so closing shows it for 5 seconds before the clock.
        IsMusicPresent = Music is not null;
        _pausedMusicEndsAt = null;
    }

    private void CloseOverlay(DateTimeOffset now)
    {
        if (_overlay == IslandOverlay.Expanded && IsMusicPresent && Music is { IsPlaying: false })
        {
            // Leaving the expanded island with paused music: its 5 seconds start now (section 13).
            _pausedMusicEndsAt = now + PausedMusicDuration;
        }

        _overlay = IslandOverlay.None;
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
