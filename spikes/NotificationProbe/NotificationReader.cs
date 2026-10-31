// NotificationReader.cs: turns a Windows UserNotification into a NotificationSnapshot.
// Kept separate because some apps throw on parts of this data, and each part fails on its own.

using Windows.UI.Notifications;

namespace NotificationProbe;

internal static class NotificationReader
{
    public static NotificationSnapshot Read(UserNotification notification)
    {
        return new NotificationSnapshot(
            notification.Id,
            ReadOrFallback(() => notification.AppInfo.DisplayInfo.DisplayName),
            ReadOrFallback(() => notification.AppInfo.AppUserModelId),
            notification.CreationTime,
            ReadTexts(notification));
    }

    private static IReadOnlyList<string> ReadTexts(UserNotification notification)
    {
        try
        {
            NotificationBinding? binding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (binding is null)
            {
                return [];
            }

            return binding.GetTextElements().Select(element => element.Text ?? string.Empty).ToList();
        }
        catch (Exception error)
        {
            return [$"<could not read text: {error.GetType().Name} 0x{error.HResult:X8}>"];
        }
    }

    private static string ReadOrFallback(Func<string> read)
    {
        try
        {
            return read();
        }
        catch (Exception error)
        {
            return $"<unavailable: {error.GetType().Name} 0x{error.HResult:X8}>";
        }
    }
}
