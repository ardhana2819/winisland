// IslandCallTests.cs: checks the WhatsApp Web call banner (PRD FR-38, AC-29): which notifications
// are calls, that the banner stays until Windows removes the call, comes before message banners,
// can be hidden without ending the call, and that opening the island during a call shows the
// notifications page.

using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Island;

public class IslandCallTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 21, 22, 0, TimeSpan.FromHours(7));

    [Theory]
    [InlineData("\U0001F4DE Incoming voice call", true, false)]
    [InlineData("\U0001F4F9 Incoming video call", true, true)]
    [InlineData("\U0001F4DE Missed voice call", false, false)]
    [InlineData("Incoming voice call later?", false, false)]
    public void WhatsApp_Web_call_texts_are_recognised(string text, bool isCall, bool isVideo)
    {
        IncomingCall? call = IncomingCall.From(WebMessage(1, "Caller", text));

        Assert.Equal(isCall, call is not null);
        Assert.Equal(isVideo, call?.IsVideo ?? false);
    }

    [Fact]
    public void Only_WhatsApp_Web_counts_as_a_call()
    {
        IslandNotification desktop = WebMessage(1, "Caller", "Incoming voice call") with { FromBrowser = false };

        Assert.Null(IncomingCall.From(desktop));
    }

    [Fact]
    public void A_call_shows_its_banner_until_Windows_removes_it()
    {
        IslandStateMachine island = new();
        island.AddNotification(WebMessage(7, "Caller", "Incoming voice call"), Now);

        Assert.Equal(IslandView.Call, island.View);
        Assert.Equal("Incoming voice call", island.Call?.Description);
        Assert.Null(island.NextTimeCheckAt);
        Assert.Empty(island.NotificationStacks);

        island.RemoveNotification(7, Now.AddSeconds(30));

        Assert.Null(island.Call);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void A_call_comes_before_a_message_banner_which_returns_afterwards()
    {
        IslandStateMachine island = new();
        island.AddNotification(WebMessage(1, "Sam", "hi"), Now);
        island.AddNotification(WebMessage(2, "Caller", "Incoming voice call"), Now);
        Assert.Equal(IslandView.Call, island.View);

        island.RemoveNotification(2, Now.AddSeconds(2));

        Assert.Equal(IslandView.Notification, island.View);
    }

    [Fact]
    public void Hotkey_3_hides_the_call_banner_but_the_call_keeps_ringing()
    {
        IslandStateMachine island = new();
        island.AddNotification(WebMessage(1, "Caller", "Incoming video call"), Now);

        island.HideNotifications(Now);

        Assert.Equal(IslandView.Compact, island.View);
        Assert.NotNull(island.Call);
    }

    [Fact]
    public void Opening_the_island_during_a_call_opens_the_notifications_page()
    {
        IslandStateMachine island = new();
        island.AddNotification(WebMessage(1, "Caller", "Incoming voice call"), Now);
        island.HideCallBanner();

        island.ToggleExpanded(Now);

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Equal(ExpandedPage.Notifications, island.Page);
    }

    [Fact]
    public void A_missed_call_arrives_as_a_normal_notification()
    {
        IslandStateMachine island = new();
        island.AddNotification(WebMessage(1, "Caller", "Incoming voice call"), Now);
        island.RemoveNotification(1, Now.AddSeconds(60));

        island.AddNotification(WebMessage(2, "Caller", "\U0001F4DE Missed voice call"), Now.AddSeconds(60));

        Assert.Equal(IslandView.Notification, island.View);
        Assert.Single(island.NotificationStacks);
    }

    private static IslandNotification WebMessage(uint id, string sender, string text)
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp Web", sender, text, Now, FromBrowser: true);
    }
}
