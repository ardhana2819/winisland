// ToastReader.cs: reads one Windows notification into Core's RawNotification.
// Same reading as spike U7's probe: app name, app id, and the text lines of the toast.
// Each part can fail on its own for some apps, so each has its own fallback. Message text stays
// in memory (PRD FR-18); errors are recorded without any of it.

using Windows.UI.Notifications;

using WinIsland.Core.Notifications;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Notifications;

internal static class ToastReader
{
    public static RawNotification Read(UserNotification notification)
    {
        return new RawNotification(
            notification.Id,
            ReadOrEmpty(() => notification.AppInfo.AppUserModelId, "app id"),
            ReadOrEmpty(() => notification.AppInfo.DisplayInfo.DisplayName, "app name"),
            ReadTexts(notification),
            notification.CreationTime);
    }

    private static IReadOnlyList<string> ReadTexts(UserNotification notification)
    {
        try
        {
            NotificationBinding? binding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            return binding is null
                ? []
                : binding.GetTextElements().Select(element => element.Text ?? string.Empty).ToList();
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Reading notification text failed", error);
            return [];
        }
    }

    private static string ReadOrEmpty(Func<string> read, string what)
    {
        try
        {
            return read() ?? string.Empty;
        }
        catch (Exception error)
        {
            PlatformLog.Problem($"Reading notification {what} failed", error);
            return string.Empty;
        }
    }
}
