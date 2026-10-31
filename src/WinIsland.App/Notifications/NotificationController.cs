// NotificationController.cs: connects the Windows notification listener to the island.
// Asks for access once at start (FR-12): if Windows says no, the island shows the permission
// prompt and keeps working as clock and music. Otherwise every new or removed notification is
// moved to the UI thread and handed to the island's rules (Core). App logos are decoded small
// and kept per app. Message text stays in memory only (FR-18).

using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;

using WinIsland.App.Music;
using WinIsland.Core.Island;
using WinIsland.Core.Notifications;
using WinIsland.Platform.Notifications;

namespace WinIsland.App.Notifications;

internal sealed class NotificationController : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<string, ImageSource?> _logos = [];
    private WindowsNotificationListener? _listener;

    public NotificationController(IslandStateMachine island, Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _island = island;
        _dispatcher = dispatcher;
    }

    /// <summary>The app's logo for a notification, or null to show the letter circle instead.</summary>
    public ImageSource? LogoFor(IslandNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return _logos.GetValueOrDefault(LogoKey(notification));
    }

    public async Task StartAsync()
    {
        try
        {
            _listener = WindowsNotificationListener.Create();
            if (!await _listener.RequestAccessAsync())
            {
                _island.ShowPermissionPrompt();
                return;
            }

            _listener.Added += OnAdded;
            _listener.Removed += OnRemoved;
            await _listener.StartAsync();
            Trace.TraceInformation("Notifications: {0}", _listener.IsInstant ? "instant (package identity)" : "checked once per second");
        }
        catch (Exception error)
        {
            // Without notifications the island still works as clock and music (section 13).
            Trace.TraceWarning("The notification listener could not be started: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
            _island.ShowPermissionPrompt();
        }
    }

    public void Dispose()
    {
        _listener?.Dispose();
    }

    private void OnAdded(object? sender, NotificationArrival arrival)
    {
        IslandNotification notification = NotificationParser.Parse(arrival.Notification);
        ImageSource? logo = arrival.AppLogo is null ? null : ArtworkImage.FromBytes(arrival.AppLogo);
        _dispatcher.BeginInvoke(() =>
        {
            _logos[LogoKey(notification)] = logo;
            _island.AddNotification(notification, DateTimeOffset.Now);
        });
    }

    private void OnRemoved(object? sender, uint windowsId)
    {
        _dispatcher.BeginInvoke(() => _island.RemoveNotification(windowsId, DateTimeOffset.Now));
    }

    private static string LogoKey(IslandNotification notification) => $"{notification.Source}\u0001{notification.AppName}";
}
