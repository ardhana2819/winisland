// NotificationPoller.cs: the fallback path. Asks Windows for all current toast notifications
// once per second and compares with what we saw last time, to find added and removed ones.

using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace NotificationProbe;

internal sealed class NotificationPoller(NotificationTracker tracker, ConsoleReport report)
{
    private const string SeenVia = "poll";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    public async Task RunAsync(CancellationToken stop)
    {
        bool firstPoll = true;
        while (!stop.IsCancellationRequested)
        {
            await PollOnceAsync(firstPoll);
            firstPoll = false;

            try
            {
                await Task.Delay(PollInterval, stop);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task PollOnceAsync(bool firstPoll)
    {
        try
        {
            IReadOnlyList<UserNotification> notifications =
                await UserNotificationListener.Current.GetNotificationsAsync(NotificationKinds.Toast);

            if (firstPoll)
            {
                report.Line($"First poll: {notifications.Count} notification(s) already in the notification center.");
            }

            HashSet<uint> stillPresent = [];
            foreach (UserNotification notification in notifications)
            {
                stillPresent.Add(notification.Id);
                tracker.Seen(NotificationReader.Read(notification), firstPoll ? "poll (already there at start)" : SeenVia);
            }

            foreach (uint id in tracker.CurrentIds().Where(id => !stillPresent.Contains(id)))
            {
                tracker.Removed(id, SeenVia);
            }
        }
        catch (Exception error)
        {
            report.Error("GetNotificationsAsync failed", error);
        }
    }
}
