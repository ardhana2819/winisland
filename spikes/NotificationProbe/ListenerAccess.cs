// ListenerAccess.cs: checks and requests permission to read notifications.
// Prints the exact status or error so spike S1 can record what happens with and without identity.

using Windows.UI.Notifications.Management;

namespace NotificationProbe;

internal static class ListenerAccess
{
    public static async Task<bool> CheckAndRequestAsync(ConsoleReport report)
    {
        UserNotificationListener listener;
        try
        {
            listener = UserNotificationListener.Current;
        }
        catch (Exception error)
        {
            report.Error("Getting UserNotificationListener.Current failed", error);
            return false;
        }

        try
        {
            report.Line($"Access status before request: {listener.GetAccessStatus()}");
        }
        catch (Exception error)
        {
            report.Error("GetAccessStatus failed", error);
        }

        try
        {
            UserNotificationListenerAccessStatus status = await listener.RequestAccessAsync();
            report.Line($"Access status after request: {status}");
            if (status != UserNotificationListenerAccessStatus.Allowed)
            {
                report.Line("Access is not allowed. Check Settings > Privacy & security > Notifications.");
            }

            return status == UserNotificationListenerAccessStatus.Allowed;
        }
        catch (Exception error)
        {
            report.Error("RequestAccessAsync failed", error);
            return false;
        }
    }
}
