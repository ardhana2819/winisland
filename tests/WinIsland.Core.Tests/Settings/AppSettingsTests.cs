// AppSettingsTests.cs: checks the settings rules (FR-19, section 12) and the saved file format.

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Core.Notifications;
using WinIsland.Core.Settings;

namespace WinIsland.Core.Tests.Settings;

public class AppSettingsTests
{
    [Fact]
    public void Defaults_follow_the_PRD_and_design_board_and_are_valid()
    {
        AppSettings defaults = AppSettings.Defaults;

        Assert.Equal(HotkeySet.Defaults, defaults.Hotkeys);
        Assert.Equal(NotificationSettings.Defaults, defaults.Notifications);
        Assert.True(defaults.LaunchAtStartup);
        Assert.Equal(IslandSize.Medium, defaults.Size);
        Assert.Empty(defaults.FindProblems());
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(6, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void Notification_duration_must_be_3_to_15_seconds(int seconds, bool valid)
    {
        AppSettings settings = AppSettings.Defaults with
        {
            Notifications = NotificationSettings.Defaults with { BannerDuration = TimeSpan.FromSeconds(seconds) },
        };

        Assert.Equal(valid, settings.FindProblems().Count == 0);
    }

    [Fact]
    public void Hotkey_problems_are_reported_as_settings_problems()
    {
        AppSettings settings = AppSettings.Defaults with
        {
            Hotkeys = HotkeySet.Defaults with { HideBanner = HotkeySet.Defaults.Expand },
        };

        Assert.Contains(settings.FindProblems(), problem => problem.Contains("both use Ctrl+Alt+I", StringComparison.Ordinal));
    }

    [Fact]
    public void Saved_settings_read_back_the_same()
    {
        AppSettings changed = new(
            new HotkeySet(
                new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, 'E'),
                new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'H'),
                new Hotkey(HotkeyModifiers.Alt, 'Z')),
            new NotificationSettings(ShowWhatsApp: true, ShowDiscord: false, ShowOtherApps: true, HideMessageText: true, TimeSpan.FromSeconds(10)),
            LaunchAtStartup: false,
            IslandSize.ExtraLarge);

        AppSettings readBack = AppSettingsJson.Read(AppSettingsJson.Write(changed));

        Assert.Equal(changed, readBack);
    }

    [Fact]
    public void The_file_is_readable_and_holds_no_message_content()
    {
        string json = AppSettingsJson.Write(AppSettings.Defaults);

        Assert.Contains("\"ExpandHotkey\": \"Ctrl+Alt+I\"", json, StringComparison.Ordinal);
        Assert.Contains("\"NotificationSeconds\": 6", json, StringComparison.Ordinal);
        Assert.Contains("\"IslandSize\": \"Medium\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Preview", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sender", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json at all")]
    [InlineData("[1, 2, 3]")]
    public void A_missing_or_damaged_file_gives_the_defaults(string? json)
    {
        Assert.Equal(AppSettings.Defaults, AppSettingsJson.Read(json));
    }

    [Fact]
    public void Missing_or_broken_values_fall_back_one_by_one()
    {
        const string json = """
            {
              "ExpandHotkey": "Ctrl+Shift+E",
              "HideShowHotkey": "nonsense",
              "NotificationSeconds": 99,
              "ShowDiscord": false
            }
            """;

        AppSettings read = AppSettingsJson.Read(json);

        Assert.Equal("Ctrl+Shift+E", read.Hotkeys.Expand.ToString());
        Assert.Equal(HotkeySet.Defaults.HideShow, read.Hotkeys.HideShow);
        Assert.Equal(TimeSpan.FromSeconds(6), read.Notifications.BannerDuration);
        Assert.False(read.Notifications.ShowDiscord);
        Assert.True(read.Notifications.ShowWhatsApp);
        Assert.True(read.LaunchAtStartup);
    }

    [Theory]
    [InlineData("\"Large\"", IslandSize.Large)]
    [InlineData("\"extralarge\"", IslandSize.ExtraLarge)]
    [InlineData("\"Huge\"", IslandSize.Medium)]
    [InlineData("\"3\"", IslandSize.Medium)]
    [InlineData("\"\"", IslandSize.Medium)]
    [InlineData("null", IslandSize.Medium)]
    public void The_island_size_is_read_by_name_and_anything_else_is_Medium(string value, IslandSize expected)
    {
        string json = "{ \"IslandSize\": " + value + " }";

        Assert.Equal(expected, AppSettingsJson.Read(json).Size);
    }

    [Fact]
    public void An_unknown_island_size_is_a_problem()
    {
        AppSettings broken = AppSettings.Defaults with { Size = (IslandSize)99 };

        Assert.Single(broken.FindProblems());
    }

    [Fact]
    public void Clashing_hotkeys_in_the_file_fall_back_to_the_default_hotkeys()
    {
        const string json = """{ "ExpandHotkey": "Alt+X", "HideNotificationsHotkey": "Alt+X" }""";

        Assert.Equal(HotkeySet.Defaults, AppSettingsJson.Read(json).Hotkeys);
    }
}
