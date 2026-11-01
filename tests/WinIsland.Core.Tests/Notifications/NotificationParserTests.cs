// NotificationParserTests.cs: checks how Windows toasts become island notifications (FR-13, section 13),
// using the layouts seen in spike U7. Sender and message values here are made up.

using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class NotificationParserTests
{
    private const string WhatsAppId = "5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App";
    private const string DiscordId = "com.squirrel.Discord.Discord";
    private static readonly DateTimeOffset At = new(2026, 10, 31, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void WhatsApp_first_line_is_the_chat_and_the_rest_is_the_message()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw(WhatsAppId, "WhatsApp", "Sam", "See you at 8"));

        Assert.Equal(NotificationSource.WhatsApp, parsed.Source);
        Assert.Equal("Sam", parsed.Sender);
        Assert.Equal("See you at 8", parsed.Preview);
        Assert.Equal("WhatsApp", parsed.AppName);
        Assert.Equal(At, parsed.ReceivedAt);
    }

    [Fact]
    public void WhatsApp_group_keeps_the_sender_prefix_in_the_message()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw(WhatsAppId, "WhatsApp", "Family", "~ Alex: dinner is ready"));

        Assert.Equal("Family", parsed.Sender);
        Assert.Equal("~ Alex: dinner is ready", parsed.Preview);
    }

    [Fact]
    public void Discord_is_recognised_by_its_app_id()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw(DiscordId, "Discord", "Kai", "gg"));

        Assert.Equal(NotificationSource.Discord, parsed.Source);
        Assert.Equal("Kai", parsed.Sender);
        Assert.Equal("gg", parsed.Preview);
    }

    [Fact]
    public void Other_apps_are_marked_as_other()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw("Microsoft.ScreenSketch_8wekyb3d8bbwe!App", "Snipping Tool", "Screenshot copied"));

        Assert.Equal(NotificationSource.Other, parsed.Source);
    }

    [Fact]
    public void A_single_line_becomes_the_sender_with_no_preview()
    {
        // Spike U7: a WhatsApp photo arrives as one line.
        IslandNotification parsed = NotificationParser.Parse(Raw(WhatsAppId, "WhatsApp", "WhatsApp Image 2026-10-31 at 09.08.55"));

        Assert.Equal("WhatsApp Image 2026-10-31 at 09.08.55", parsed.Sender);
        Assert.Equal(string.Empty, parsed.Preview);
    }

    [Fact]
    public void No_text_shows_the_app_only()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw(DiscordId, "Discord", "  ", ""));

        Assert.Equal("Discord", parsed.Sender);
        Assert.Equal(string.Empty, parsed.Preview);
    }

    [Fact]
    public void Extra_lines_are_joined_and_a_missing_app_name_is_filled_in()
    {
        IslandNotification parsed = NotificationParser.Parse(Raw("Some.App", " ", "Title", "line one", "line two"));

        Assert.Equal("line one line two", parsed.Preview);
        Assert.Equal("App", parsed.AppName);
    }

    [Fact]
    public void Line_breaks_inside_a_message_become_single_spaces()
    {
        // Diva's bug, 1 Nov 2026: a long group message with its own line breaks spilled out of its card.
        IslandNotification parsed = NotificationParser.Parse(Raw(WhatsAppId, "WhatsApp", "Runners\r\n", "Alex: jam 5 sore\n\nsetuju?\r\n  @Kai   @Sam\t"));

        Assert.Equal("Runners", parsed.Sender);
        Assert.Equal("Alex: jam 5 sore setuju? @Kai @Sam", parsed.Preview);
    }

    [Fact]
    public void A_long_message_arrives_complete()
    {
        // Spike U7: WhatsApp sent a 3,214 character message in full; the island cuts it, not the parser.
        string longText = new('x', 3214);
        IslandNotification parsed = NotificationParser.Parse(Raw(WhatsAppId, "WhatsApp", "Sam", longText));

        Assert.Equal(3214, parsed.Preview.Length);
    }

    private static RawNotification Raw(string appId, string appName, params string[] texts)
    {
        return new RawNotification(7, appId, appName, texts, At);
    }
}
