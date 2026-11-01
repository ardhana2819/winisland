// IslandNotificationRulesTests.cs: checks how notifications change the island (FR-2, FR-6, FR-6a,
// FR-14, FR-15, FR-16, FR-21, FR-22): banners, the notifications page, hotkey 3, swipe, pause and hide.

using WinIsland.Core.Island;
using WinIsland.Core.Music;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Island;

public class IslandNotificationRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 31, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void A_notification_shows_a_banner_for_6_seconds_then_the_resting_view()
    {
        IslandStateMachine island = new();

        island.AddNotification(Message(1, "Sam"), Now);
        Assert.Equal(IslandView.Notification, island.View);
        Assert.Equal(Now.AddSeconds(6), island.NextTimeCheckAt);

        island.UpdateTime(Now.AddSeconds(6));
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Single(island.RecentNotifications);
    }

    [Fact]
    public void A_banner_replaces_the_expanded_island_and_returns_to_music_afterwards()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.AddNotification(Message(1, "Sam"), Now);
        Assert.Equal(IslandView.Notification, island.View);

        island.UpdateTime(Now.AddSeconds(6));
        Assert.Equal(IslandView.Music, island.View);
    }

    [Fact]
    public void Expanding_replaces_the_banner()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.AddNotification(Message(1, "Sam"), Now);

        island.ToggleExpanded(Now.AddSeconds(1));

        Assert.Equal(IslandView.Expanded, island.View);
    }

    [Fact]
    public void Tapping_the_banner_opens_the_notifications_page_and_stops_the_banner_timer()
    {
        // v1.1 (FR-15, FR-21): the banner opens the expanded island's notifications page, not a separate list.
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.OpenNotificationsPage();

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Equal(ExpandedPage.Notifications, island.Page);
        Assert.Empty(island.WaitingBanners);
        Assert.Null(island.NextTimeCheckAt);
        Assert.Equal(2, island.RecentNotifications.Count);
    }

    [Fact]
    public void The_notifications_page_closes_like_the_expanded_island()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationsPage();

        island.Collapse(Now.AddSeconds(3));

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void The_notifications_page_stays_open_and_empty_when_its_last_item_is_read()
    {
        // v1.1 (FR-22): replaces the v1.0 rule that closed the list; the page says "No notifications".
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationsPage();

        island.RemoveNotification(1, Now.AddSeconds(2));
        island.UpdateTime(Now.AddSeconds(3));

        Assert.Empty(island.RecentNotifications);
        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Equal(ExpandedPage.Notifications, island.Page);
    }

    [Fact]
    public void A_notification_on_the_notifications_page_goes_onto_the_page_without_a_banner()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationsPage();

        island.AddNotification(Message(2, "Kai"), Now.AddSeconds(1));

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Equal(ExpandedPage.Notifications, island.Page);
        Assert.Empty(island.WaitingBanners);
        Assert.Null(island.NextTimeCheckAt);
        Assert.Equal("Kai", island.RecentNotifications[0].Sender);
    }

    [Fact]
    public void A_notification_on_the_music_page_still_replaces_it_with_a_banner()
    {
        IslandStateMachine island = new();
        island.ToggleExpanded(Now);

        island.AddNotification(Message(1, "Sam"), Now);

        Assert.Equal(IslandView.Notification, island.View);
    }

    [Fact]
    public void Pages_switch_only_while_expanded_and_expanding_opens_the_music_page()
    {
        IslandStateMachine island = new();
        island.ShowPage(ExpandedPage.Notifications);
        Assert.Equal(ExpandedPage.Music, island.Page);
        Assert.Equal(IslandView.Compact, island.View);

        island.ToggleExpanded(Now);
        island.ShowPage(ExpandedPage.Notifications);
        Assert.Equal(ExpandedPage.Notifications, island.Page);

        int changes = 0;
        island.Changed += (_, _) => changes++;
        island.ShowPage(ExpandedPage.Notifications);
        Assert.Equal(0, changes);

        island.Collapse(Now);
        island.ToggleExpanded(Now);
        Assert.Equal(ExpandedPage.Music, island.Page);
    }

    [Fact]
    public void Hotkey_3_closes_the_notifications_page_but_not_the_music_page()
    {
        IslandStateMachine island = new();
        island.ToggleExpanded(Now);

        island.HideNotifications(Now);
        Assert.Equal(IslandView.Expanded, island.View);

        island.ShowPage(ExpandedPage.Notifications);
        island.HideNotifications(Now);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Swipe_shows_the_next_banner_then_closes_when_none_are_left()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.SwipeBanner(Now.AddSeconds(1));
        Assert.Equal(IslandView.Notification, island.View);
        Assert.Equal(1u, island.WaitingBanners[0].WindowsId);

        island.SwipeBanner(Now.AddSeconds(2));
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Equal(2, island.RecentNotifications.Count);
    }

    [Fact]
    public void Hotkey_3_hides_all_banners_and_the_notifications_page_without_deleting_anything()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.HideNotifications(Now);
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Empty(island.WaitingBanners);

        island.AddNotification(Message(3, "Ana"), Now.AddSeconds(1));
        island.OpenNotificationsPage();
        island.HideNotifications(Now.AddSeconds(2));

        Assert.Equal(IslandView.Compact, island.View);
        Assert.Equal(3, island.RecentNotifications.Count);
    }

    [Fact]
    public void Notifications_are_dropped_while_paused()
    {
        IslandStateMachine island = new();
        island.Pause(Now);

        island.AddNotification(Message(1, "Sam"), Now.AddMinutes(1));
        island.Resume();

        Assert.Empty(island.RecentNotifications);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Pausing_hides_waiting_banners()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);

        island.Pause(Now.AddSeconds(1));
        island.Resume();

        Assert.Equal(IslandView.Compact, island.View);
        Assert.Empty(island.WaitingBanners);
    }

    [Fact]
    public void Notifications_are_still_processed_while_hidden_and_show_when_shown_again()
    {
        IslandStateMachine island = new();
        island.ToggleHidden(Now);

        island.AddNotification(Message(1, "Sam"), Now);
        Assert.Equal(IslandView.Compact, island.View);

        island.ToggleHidden(Now.AddSeconds(2));
        Assert.Equal(IslandView.Notification, island.View);
    }

    [Fact]
    public void Disabled_apps_are_ignored()
    {
        IslandStateMachine island = new();

        island.AddNotification(Message(1, "Snipping Tool") with { Source = NotificationSource.Other }, Now);
        Assert.Empty(island.RecentNotifications);

        island.UpdateNotificationSettings(NotificationSettings.Defaults with { ShowWhatsApp = false });
        island.AddNotification(Message(2, "Sam"), Now);
        Assert.Empty(island.RecentNotifications);
    }

    [Fact]
    public void Banner_duration_follows_the_setting()
    {
        IslandStateMachine island = new();
        island.UpdateNotificationSettings(NotificationSettings.Defaults with { BannerDuration = TimeSpan.FromSeconds(10) });

        island.AddNotification(Message(1, "Sam"), Now);

        Assert.Equal(Now.AddSeconds(10), island.NextTimeCheckAt);
    }

    [Fact]
    public void Reading_the_message_in_the_app_dismisses_its_banner()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);

        island.RemoveNotification(1, Now.AddSeconds(2));
        island.UpdateTime(Now.AddSeconds(3));

        Assert.Equal(IslandView.Compact, island.View);
        Assert.Empty(island.RecentNotifications);
    }

    [Fact]
    public void Swipe_and_banner_tap_do_nothing_without_a_banner()
    {
        IslandStateMachine island = new();

        island.SwipeBanner(Now);
        island.OpenNotificationsPage();

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Permission_prompt_shows_and_dismisses_and_the_clock_comes_back()
    {
        IslandStateMachine island = new();

        island.ShowPermissionPrompt();
        Assert.Equal(IslandView.PermissionPrompt, island.View);

        island.DismissPermissionPrompt();
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Hotkey_conflict_banner_comes_before_the_permission_prompt()
    {
        IslandStateMachine island = new();
        island.ShowPermissionPrompt();
        island.ShowHotkeyConflict("Alt+X is used by another app");

        Assert.Equal(IslandView.HotkeyConflict, island.View);

        island.DismissHotkeyConflict();
        Assert.Equal(IslandView.PermissionPrompt, island.View);
    }

    private static IslandNotification Message(uint id, string sender)
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", sender, "hello", Now);
    }

    private static IslandStateMachine WithMusicPlaying()
    {
        IslandStateMachine island = new();
        island.UpdateMusic(new NowPlaying("Song", "Artist", PlaybackStatus.Playing, TimeSpan.FromMinutes(3), TimeSpan.Zero, Now, canSeek: true), Now);
        return island;
    }
}
