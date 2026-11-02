// IslandChatTests.cs: checks opening a stack on the notifications page and going back (PRD FR-35,
// AC-26b), and which hidden banners are reported for clearing from Windows (FR-36, AC-27).

using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Island;

public class IslandChatTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void A_stack_of_two_or_more_opens_and_a_click_goes_back()
    {
        IslandStateMachine island = OnNotificationsPage(Message(1, "Sam"), Message(2, "Sam"));

        island.OpenChatOf(Message(2, "Sam"));
        Assert.Equal(2, island.OpenChat?.Count);

        island.CloseChat();
        Assert.Null(island.OpenChat);
    }

    [Fact]
    public void A_single_message_does_not_open()
    {
        IslandStateMachine island = OnNotificationsPage(Message(1, "Sam"));

        island.OpenChatOf(Message(1, "Sam"));

        Assert.Null(island.OpenChat);
    }

    [Fact]
    public void A_chat_opens_only_on_the_open_notifications_page()
    {
        IslandStateMachine island = new();
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Sam"), Now);

        island.OpenChatOf(Message(2, "Sam"));

        Assert.Null(island.OpenChat);
    }

    [Fact]
    public void Switching_page_or_closing_the_island_goes_back_to_the_list()
    {
        IslandStateMachine island = OnNotificationsPage(Message(1, "Sam"), Message(2, "Sam"));
        island.OpenChatOf(Message(2, "Sam"));
        island.ShowPage(ExpandedPage.Music);
        island.ShowPage(ExpandedPage.Notifications);
        Assert.Null(island.OpenChat);

        island.OpenChatOf(Message(2, "Sam"));
        island.Collapse(Now);
        Assert.Null(island.OpenChat);
    }

    [Fact]
    public void Reading_the_open_chat_in_the_app_goes_back_to_the_list()
    {
        IslandStateMachine island = OnNotificationsPage(Message(1, "Sam"), Message(2, "Sam"), Message(3, "Kai"));
        island.OpenChatOf(Message(2, "Sam"));

        island.RemoveNotification(2, Now);
        island.UpdateTime(Now.AddSeconds(1));

        Assert.Null(island.OpenChat);
        Assert.Equal("Kai", Assert.Single(island.NotificationStacks).Newest.Sender);
    }

    [Fact]
    public void A_new_message_in_the_open_chat_keeps_it_open_with_the_new_count()
    {
        IslandStateMachine island = OnNotificationsPage(Message(1, "Sam"), Message(2, "Sam"));
        island.OpenChatOf(Message(2, "Sam"));

        island.AddNotification(Message(3, "Sam"), Now);

        Assert.Equal(3, island.OpenChat?.Count);
    }

    [Fact]
    public void A_swipe_reports_the_hidden_banner()
    {
        IslandStateMachine island = new();
        List<IslandNotification> reported = [];
        island.BannersDismissed += (_, hidden) => reported.AddRange(hidden);
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.SwipeBanner(Now);

        Assert.Equal(2u, Assert.Single(reported).WindowsId);
    }

    [Fact]
    public void Hotkey_3_on_banners_reports_every_waiting_banner()
    {
        IslandStateMachine island = new();
        List<IslandNotification> reported = [];
        island.BannersDismissed += (_, hidden) => reported.AddRange(hidden);
        island.AddNotification(Message(1, "Sam"), Now);
        island.AddNotification(Message(2, "Kai"), Now);

        island.HideNotifications(Now);

        Assert.Equal(new uint[] { 2, 1 }, reported.Select(item => item.WindowsId));
    }

    [Fact]
    public void Hotkey_3_on_the_page_and_the_banner_timer_report_nothing()
    {
        IslandStateMachine island = new();
        int reports = 0;
        island.BannersDismissed += (_, _) => reports++;
        island.AddNotification(Message(1, "Sam"), Now);
        island.UpdateTime(Now.AddSeconds(6));

        island.AddNotification(Message(2, "Kai"), Now.AddSeconds(10));
        island.OpenNotificationsPage();
        island.HideNotifications(Now.AddSeconds(11));

        Assert.Equal(0, reports);
    }

    private static IslandStateMachine OnNotificationsPage(params IslandNotification[] messages)
    {
        IslandStateMachine island = new();
        foreach (IslandNotification message in messages)
        {
            island.AddNotification(message, Now);
        }

        island.OpenNotificationsPage();
        return island;
    }

    private static IslandNotification Message(uint id, string sender)
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", sender, $"message {id}", Now);
    }
}
