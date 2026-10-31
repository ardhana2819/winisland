// NotificationFeedTests.cs: checks the last 3 list, the banner timer, and the replacement rule
// (FR-15, FR-16, Diva's decision on 31 Oct 2026, spike U3 and U7).

using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class NotificationFeedTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 31, 20, 0, 0, TimeSpan.FromHours(7));
    private static readonly TimeSpan Six = TimeSpan.FromSeconds(6);

    [Fact]
    public void Keeps_the_last_3_newest_first()
    {
        NotificationFeed feed = new();
        for (uint id = 1; id <= 4; id++)
        {
            feed.Add(Message(id, $"Person {id}"), Now, Six);
        }

        Assert.Equal(new uint[] { 4, 3, 2 }, feed.Recent.Select(item => item.WindowsId));
    }

    [Fact]
    public void Each_new_notification_restarts_one_shared_banner_timer()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Kai"), Now.AddSeconds(4), Six);

        Assert.Equal(Now.AddSeconds(10), feed.BannerEndsAt);
        Assert.Equal(2u, feed.CurrentBanner?.WindowsId);
        Assert.Equal(2, feed.Waiting.Count);
    }

    [Fact]
    public void When_the_timer_ends_all_banners_hide_but_the_list_stays()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Kai"), Now, Six);

        Assert.False(feed.UpdateTime(Now.AddSeconds(5.9)));
        Assert.True(feed.UpdateTime(Now.AddSeconds(6)));

        Assert.Empty(feed.Waiting);
        Assert.Null(feed.BannerEndsAt);
        Assert.Equal(2, feed.Recent.Count);
    }

    [Fact]
    public void The_same_notification_seen_twice_is_added_once()
    {
        NotificationFeed feed = new();

        Assert.True(feed.Add(Message(1, "Sam"), Now, Six));
        Assert.False(feed.Add(Message(1, "Sam"), Now, Six));
        Assert.Single(feed.Recent);
    }

    [Fact]
    public void Swipe_hides_the_current_banner_and_shows_the_next_with_a_fresh_timer()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Kai"), Now, Six);

        feed.HideCurrentBanner(Now.AddSeconds(3), Six);
        Assert.Equal(1u, feed.CurrentBanner?.WindowsId);
        Assert.Equal(Now.AddSeconds(9), feed.BannerEndsAt);

        feed.HideCurrentBanner(Now.AddSeconds(4), Six);
        Assert.Null(feed.CurrentBanner);
        Assert.Null(feed.BannerEndsAt);
        Assert.Equal(2, feed.Recent.Count);
    }

    [Fact]
    public void A_removal_with_nothing_after_it_means_read_and_the_whole_chat_leaves()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Kai"), Now, Six);

        feed.MarkRemoved(1, Now.AddSeconds(2));
        Assert.Equal(Now.AddSeconds(3), feed.NextTimeCheckAt);

        // Decided 1 second later, not before.
        Assert.False(feed.UpdateTime(Now.AddSeconds(2.5)));
        Assert.True(feed.UpdateTime(Now.AddSeconds(3)));
        Assert.Equal(new uint[] { 2 }, feed.Recent.Select(item => item.WindowsId));
        Assert.Equal(new uint[] { 2 }, feed.Waiting.Select(item => item.WindowsId));
    }

    [Fact]
    public void WhatsApp_swapping_its_notification_keeps_the_older_message()
    {
        // Spike U7: REMOVED 511, ADDED 512 from the same chat within about 50 ms.
        NotificationFeed feed = new();
        feed.Add(Message(511, "Sam", "1"), Now, Six);

        feed.MarkRemoved(511, Now.AddSeconds(1));
        feed.Add(Message(512, "Sam", "2"), Now.AddSeconds(1.05), Six);
        feed.UpdateTime(Now.AddSeconds(5));

        Assert.Equal(new uint[] { 512, 511 }, feed.Recent.Select(item => item.WindowsId));
    }

    [Fact]
    public void Reading_a_chat_after_replacements_removes_all_its_messages()
    {
        NotificationFeed feed = new();
        feed.Add(Message(511, "Sam", "1"), Now, Six);
        feed.MarkRemoved(511, Now.AddSeconds(1));
        feed.Add(Message(512, "Sam", "2"), Now.AddSeconds(1.05), Six);
        feed.Add(Message(600, "Kai", "hi"), Now.AddSeconds(2), Six);

        // Diva opens Sam's chat: WhatsApp removes its latest notification and adds nothing.
        feed.MarkRemoved(512, Now.AddSeconds(10));
        feed.UpdateTime(Now.AddSeconds(11));

        Assert.Equal(new uint[] { 600 }, feed.Recent.Select(item => item.WindowsId));
    }

    [Fact]
    public void A_new_message_from_another_chat_does_not_count_as_a_replacement()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);

        feed.MarkRemoved(1, Now);
        feed.Add(Message(2, "Kai"), Now.AddSeconds(0.5), Six);
        feed.UpdateTime(Now.AddSeconds(1));

        Assert.Equal(new uint[] { 2 }, feed.Recent.Select(item => item.WindowsId));
    }

    [Fact]
    public void Same_sender_name_in_another_app_is_another_conversation()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Sam") with { Source = NotificationSource.Discord, AppName = "Discord" }, Now, Six);

        feed.MarkRemoved(1, Now);
        feed.UpdateTime(Now.AddSeconds(1));

        Assert.Equal(new uint[] { 2 }, feed.Recent.Select(item => item.WindowsId));
    }

    [Fact]
    public void Removing_an_unknown_notification_changes_nothing()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);

        feed.MarkRemoved(99, Now);

        Assert.Equal(Now.AddSeconds(6), feed.NextTimeCheckAt);
        Assert.False(feed.UpdateTime(Now.AddSeconds(1)));
    }

    [Fact]
    public void Next_time_check_is_the_earliest_of_banner_end_and_read_checks()
    {
        NotificationFeed feed = new();
        Assert.Null(feed.NextTimeCheckAt);

        feed.Add(Message(1, "Sam"), Now, Six);
        feed.MarkRemoved(1, Now.AddSeconds(1));

        Assert.Equal(Now.AddSeconds(2), feed.NextTimeCheckAt);
    }

    private static IslandNotification Message(uint id, string sender, string text = "hello")
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", sender, text, Now);
    }
}
