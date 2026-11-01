// WhatsAppWebPriorityTests.cs: checks that WhatsApp Web comes first when WhatsApp desktop also
// sends notifications (FR-29, AC-17, AC-18), following the WhatsApp Web spike (W4, W5, W7).

using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class WhatsAppWebPriorityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 17, 49, 0, TimeSpan.FromHours(7));
    private static readonly TimeSpan Six = TimeSpan.FromSeconds(6);

    [Fact]
    public void Desktop_closed_summary_is_ignored()
    {
        // W4: with desktop closed, Windows shows "You may have new messages" next to the Web message.
        NotificationFeed feed = new();

        bool summaryAdded = feed.Add(Desktop(1, WhatsAppWebPriority.DesktopClosedSummary, string.Empty), Now, Six);
        bool webAdded = feed.Add(Web(2, "Sam", "dark"), Now, Six);

        Assert.False(summaryAdded);
        Assert.True(webAdded);
        Assert.Single(feed.Recent);
        Assert.True(feed.Recent[0].FromBrowser);
    }

    [Fact]
    public void Web_first_then_desktop_shows_one_message_from_Web()
    {
        // W5: both open, Web 80 ms before desktop.
        NotificationFeed feed = new();

        feed.Add(Web(1, "Sam", "dark"), Now, Six);
        bool desktopAdded = feed.Add(Desktop(2, "Sam", "dark"), Now.AddMilliseconds(80), Six);

        Assert.False(desktopAdded);
        Assert.Single(feed.Recent);
        Assert.Single(feed.Waiting);
        Assert.True(feed.Recent[0].FromBrowser);
    }

    [Fact]
    public void Desktop_first_then_Web_shows_one_message_from_Web()
    {
        // W5: both open, desktop 10 ms before Web.
        NotificationFeed feed = new();

        feed.Add(Desktop(1, "Sam", "yel"), Now, Six);
        bool webAdded = feed.Add(Web(2, "Sam", "yel"), Now.AddMilliseconds(10), Six);

        Assert.False(webAdded);
        Assert.Single(feed.Recent);
        Assert.Single(feed.Waiting);
        Assert.Equal(2u, feed.Recent[0].WindowsId);
        Assert.True(feed.Recent[0].FromBrowser);
    }

    [Fact]
    public void Reading_in_desktop_still_removes_the_message_although_desktop_sends_it_again()
    {
        // W7: reading in desktop removes the Web notification, then desktop sends a longer text
        // for the same chat. That must not count as a swap, so the chat still counts as read.
        NotificationFeed feed = new();
        feed.Add(Web(1, "Runners", "Alex: jam 5?"), Now, Six);

        feed.MarkRemoved(1, Now.AddSeconds(6));
        bool desktopAdded = feed.Add(Desktop(2, "Runners", new string('x', 611)), Now.AddSeconds(6.1), Six);
        feed.UpdateTime(Now.AddSeconds(7.1));

        Assert.False(desktopAdded);
        Assert.Empty(feed.Recent);
    }

    [Fact]
    public void Desktop_shows_as_before_for_chats_Web_has_not_covered()
    {
        NotificationFeed feed = new();
        feed.Add(Web(1, "Sam", "hi"), Now, Six);

        bool otherChat = feed.Add(Desktop(2, "Kai", "hello"), Now.AddSeconds(1), Six);
        bool sameChatLater = feed.Add(Desktop(3, "Sam", "later"), Now + WhatsAppWebPriority.CoverWindow + TimeSpan.FromSeconds(1), Six);

        Assert.True(otherChat);
        Assert.True(sameChatLater);
        Assert.Equal(3, feed.Recent.Count);
    }

    [Fact]
    public void Discord_is_never_affected()
    {
        NotificationFeed feed = new();
        feed.Add(Web(1, "Sam", "hi"), Now, Six);

        bool added = feed.Add(new IslandNotification(2, NotificationSource.Discord, "Discord", "Sam", "gg", Now), Now, Six);

        Assert.True(added);
    }

    private static IslandNotification Desktop(uint id, string chat, string text)
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", chat, text, Now);
    }

    private static IslandNotification Web(uint id, string chat, string text)
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp Web", chat, text, Now, FromBrowser: true);
    }
}
