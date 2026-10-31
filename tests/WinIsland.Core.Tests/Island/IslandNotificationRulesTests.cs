// IslandNotificationRulesTests.cs: checks how notifications change the island (FR-2, FR-6, FR-6a,
// FR-14, FR-15, FR-16): banners, the list, hotkey 3, swipe, pause and hide.

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
    public void Tapping_the_banner_opens_the_list_and_stops_the_banner_timer()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.OpenNotificationList();

        Assert.Equal(IslandView.NotificationList, island.View);
        Assert.Empty(island.WaitingBanners);
        Assert.Null(island.NextTimeCheckAt);
        Assert.Equal(2, island.RecentNotifications.Count);
    }

    [Fact]
    public void The_list_closes_like_the_expanded_island()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationList();

        island.Collapse(Now.AddSeconds(3));

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void The_list_closes_when_its_last_item_is_read()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationList();

        island.RemoveNotification(1, Now.AddSeconds(2));
        island.UpdateTime(Now.AddSeconds(3));

        Assert.Empty(island.RecentNotifications);
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
    public void Hotkey_3_hides_all_banners_and_the_list_without_deleting_anything()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.HideNotifications(Now);
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Empty(island.WaitingBanners);

        island.AddNotification(Message(3, "Ana"), Now.AddSeconds(1));
        island.OpenNotificationList();
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
    public void Swipe_and_list_do_nothing_without_a_banner()
    {
        IslandStateMachine island = new();

        island.SwipeBanner(Now);
        island.OpenNotificationList();

        Assert.Equal(IslandView.Compact, island.View);
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
