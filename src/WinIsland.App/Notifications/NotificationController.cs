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
    private static readonly TimeSpan AccessCheckInterval = TimeSpan.FromMinutes(1);

    private readonly Dictionary<string, ImageSource?> _logos = [];
    private readonly DispatcherTimer _accessCheck;
    private WindowsNotificationListener? _listener;
    private bool _isListening;
    private bool _promptShown;

    public NotificationController(IslandStateMachine island, Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _island = island;
        _dispatcher = dispatcher;

        // Once a minute: if access was turned on in Windows Settings, start without a restart; if it
        // was turned off, show the prompt once (PRD section 7: recover; FR-12).
        _accessCheck = new DispatcherTimer(DispatcherPriority.Background, dispatcher) { Interval = AccessCheckInterval };
        _accessCheck.Tick += async (_, _) => await CheckAccessAsync();
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
            if (await _listener.RequestAccessAsync())
            {
                await StartListeningAsync(_listener);
            }
            else
            {
                ShowPromptOnce();
            }
        }
        catch (Exception error)
        {
            // Without notifications the island still works as clock and music (section 13).
            Trace.TraceWarning("The notification listener could not be started: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
            ShowPromptOnce();
        }

        _accessCheck.Start();
    }

    public void Dispose()
    {
        _accessCheck.Stop();
        _listener?.Dispose();
    }

    private async Task CheckAccessAsync()
    {
        if (_listener is null)
        {
            return;
        }

        try
        {
            bool allowed = _listener.IsAccessAllowed();
            if (allowed && !_isListening)
            {
                Trace.TraceInformation("Notification access was turned on; starting to listen");
                await StartListeningAsync(_listener);
            }
            else if (!allowed && _isListening)
            {
                Trace.TraceWarning("Notification access was turned off in Windows");
                ShowPromptOnce();
            }
        }
        catch (Exception error)
        {
            Trace.TraceWarning("Checking notification access failed: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
        }
    }

    private async Task StartListeningAsync(WindowsNotificationListener listener)
    {
        listener.Added += OnAdded;
        listener.Removed += OnRemoved;
        await listener.StartAsync();
        _isListening = true;
        Trace.TraceInformation("Notifications: {0}", listener.IsInstant ? "instant (package identity)" : "checked once per second");
    }

    private void ShowPromptOnce()
    {
        // Section 13: a one-time hint, not a reminder every minute.
        if (_promptShown)
        {
            return;
        }

        _promptShown = true;
        _island.ShowPermissionPrompt();
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
