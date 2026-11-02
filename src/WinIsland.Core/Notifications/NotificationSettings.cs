// NotificationSettings.cs: the notification choices from PRD section 4 and FR-14 to FR-17, with
// their defaults: WhatsApp and Discord on, other Windows apps off (design board), message text
// shown, banners hide after 6 seconds. The Settings window (Phase 5) will let Diva change them.
// v1.4 (FR-36): "Also clear from Windows notification center", off by default. When on, hiding a
// banner with a swipe or hotkey 3 also removes that chat's notifications from Windows.

namespace WinIsland.Core.Notifications;

public sealed record NotificationSettings(
    bool ShowWhatsApp,
    bool ShowDiscord,
    bool ShowOtherApps,
    bool HideMessageText,
    TimeSpan BannerDuration,
    bool ClearFromWindows = false)
{
    public static NotificationSettings Defaults { get; } = new(
        ShowWhatsApp: true,
        ShowDiscord: true,
        ShowOtherApps: false,
        HideMessageText: false,
        BannerDuration: TimeSpan.FromSeconds(6));

    public bool IsShown(NotificationSource source)
    {
        return source switch
        {
            NotificationSource.WhatsApp => ShowWhatsApp,
            NotificationSource.Discord => ShowDiscord,
            NotificationSource.Other => ShowOtherApps,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown notification source."),
        };
    }

    /// <summary>The preview the island may show: none at all when "Hide message text" is on (FR-17).</summary>
    public string PreviewFor(IslandNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return HideMessageText ? string.Empty : notification.Preview;
    }
}
