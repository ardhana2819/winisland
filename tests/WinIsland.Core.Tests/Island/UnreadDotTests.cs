// UnreadDotTests.cs: checks when the idle island shows the unread dot (FR-33, AC-23): a new
// notification counts until the notifications page has been open with it on it; a banner alone
// does not count, and reading it in the app removes it.

using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Island;

public class UnreadDotTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void A_new_notification_is_unread_also_after_its_banner_ends()
    {
        IslandStateMachine island = new();

        island.AddNotification(Message(1, "Sam"), Now);
        island.UpdateTime(Now.AddSeconds(6));

        Assert.Equal(IslandView.Compact, island.View);
        Assert.True(island.HasUnseenNotifications);
    }

    [Fact]
    public void Tapping_the_banner_opens_the_page_and_clears_the_dot()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);

        island.OpenNotificationsPage();

        Assert.False(island.HasUnseenNotifications);
    }

    [Fact]
    public void Only_the_notifications_page_counts_not_the_music_page()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.UpdateTime(Now.AddSeconds(6));

        island.ToggleExpanded(Now.AddSeconds(7));
        Assert.True(island.HasUnseenNotifications);

        island.ShowPage(ExpandedPage.Notifications);
        Assert.False(island.HasUnseenNotifications);
    }

    [Fact]
    public void A_notification_that_arrives_on_the_open_page_is_already_seen()
    {
        IslandStateMachine island = new();
        island.ToggleExpanded(Now);
        island.ShowPage(ExpandedPage.Notifications);

        island.AddNotification(Message(1, "Sam"), Now.AddSeconds(1));

        Assert.False(island.HasUnseenNotifications);
    }

    [Fact]
    public void Reading_the_message_in_the_app_clears_the_dot()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.UpdateTime(Now.AddSeconds(6));

        island.RemoveNotification(1, Now.AddSeconds(10));
        island.UpdateTime(Now.AddSeconds(11));

        Assert.Empty(island.RecentNotifications);
        Assert.False(island.HasUnseenNotifications);
    }

    [Fact]
    public void A_new_message_after_seeing_the_page_brings_the_dot_back()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationsPage();
        island.Collapse(Now.AddSeconds(2));

        island.AddNotification(Message(2, "Kai"), Now.AddSeconds(3));

        Assert.True(island.HasUnseenNotifications);
    }

    [Fact]
    public void A_repost_of_a_seen_message_stays_seen()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.OpenNotificationsPage();
        island.Collapse(Now.AddSeconds(2));

        // WhatsApp posts the same message again under a new id (repost rule).
        island.AddNotification(Message(2, "Sam"), Now.AddSeconds(10));

        Assert.Equal(2u, Assert.Single(island.RecentNotifications).WindowsId);
        Assert.False(island.HasUnseenNotifications);
    }

    [Fact]
    public void A_repost_of_an_unseen_message_stays_unseen()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);

        island.AddNotification(Message(2, "Sam"), Now.AddSeconds(10));

        Assert.True(island.HasUnseenNotifications);
    }

    [Fact]
    public void Nothing_is_unread_after_a_notification_dropped_while_paused()
    {
        IslandStateMachine island = new();
        island.Pause(Now);

        island.AddNotification(Message(1, "Sam"), Now.AddSeconds(1));

        Assert.False(island.HasUnseenNotifications);
    }

    private static IslandNotification Message(uint id, string sender, string text = "hello")
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", sender, text, Now);
    }
}
