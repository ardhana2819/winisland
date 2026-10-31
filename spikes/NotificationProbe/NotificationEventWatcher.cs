// NotificationEventWatcher.cs: tries the push path, the NotificationChanged event.
// Some reports say this event is not supported for desktop apps even with package identity.
// The spike records whether subscribing works; if it does not, the product must use polling.

using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace NotificationProbe;

internal sealed class NotificationEventWatcher(NotificationTracker tracker, ConsoleReport report)
{
    private const string SeenVia = "event";

    public void TryStart()
    {
        try
        {
            UserNotificationListener.Current.NotificationChanged += OnNotificationChanged;
            report.Line("NotificationChanged event: subscribed OK.");
        }
        catch (Exception error)
        {
            report.Error("NotificationChanged event: subscribe FAILED, only polling will be used", error);
        }
    }

    private void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        try
        {
            if (args.ChangeKind == UserNotificationChangedKind.Removed)
            {
                tracker.Removed(args.UserNotificationId, SeenVia);
                return;
            }

            UserNotification? notification = sender.GetNotification(args.UserNotificationId);
            if (notification is not null)
            {
                tracker.Seen(NotificationReader.Read(notification), SeenVia);
            }
        }
        catch (Exception error)
        {
            report.Error("Handling NotificationChanged failed", error);
        }
    }
}
