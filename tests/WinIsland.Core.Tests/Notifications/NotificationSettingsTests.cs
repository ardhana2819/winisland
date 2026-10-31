// NotificationSettingsTests.cs: checks the default notification choices (FR-14, FR-16) and "Hide message text" (FR-17).

using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class NotificationSettingsTests
{
    [Fact]
    public void Defaults_show_WhatsApp_and_Discord_only_for_6_seconds()
    {
        NotificationSettings defaults = NotificationSettings.Defaults;

        Assert.True(defaults.IsShown(NotificationSource.WhatsApp));
        Assert.True(defaults.IsShown(NotificationSource.Discord));
        Assert.False(defaults.IsShown(NotificationSource.Other));
        Assert.False(defaults.HideMessageText);
        Assert.Equal(TimeSpan.FromSeconds(6), defaults.BannerDuration);
    }

    [Fact]
    public void Hide_message_text_leaves_no_preview()
    {
        IslandNotification message = new(1, NotificationSource.WhatsApp, "WhatsApp", "Sam", "secret", DateTimeOffset.MinValue);

        Assert.Equal("secret", NotificationSettings.Defaults.PreviewFor(message));
        Assert.Equal(string.Empty, (NotificationSettings.Defaults with { HideMessageText = true }).PreviewFor(message));
    }
}
