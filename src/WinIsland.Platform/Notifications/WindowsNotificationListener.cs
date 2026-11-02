// WindowsNotificationListener.cs: reads Windows toast notifications through the Windows
// notification listener API (UserNotificationListener), with Diva's permission. The only
// notification source the PRD allows; no network.
//   FR-12 asks for access and reports the answer.
//   FR-13 reports each new notification and each removal (read in the app, spike U3).
// Spike S1: with package identity, Windows pushes changes at once (event). Without it, the event
// is not available, so this asks Windows once per second instead (polling, up to 1 s delay,
// still inside the 2 s goal G2). Either way the app sees the same Added and Removed reports.
// Notifications already in Windows when it starts are taken as known, not as new.
// FR-36 (v1.4): it can also remove notifications from Windows, only when Diva turned that on.

using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Notifications;

public sealed class WindowsNotificationListener : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);
    private const int LogoSizePixels = 64;
    private const ulong MaxLogoBytes = 512 * 1024;

    private readonly UserNotificationListener _listener;
    private readonly HashSet<uint> _known = [];
    private readonly Dictionary<string, byte[]?> _logos = [];
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _stop = new();
    private bool _usesEvent;

    private WindowsNotificationListener(UserNotificationListener listener)
    {
        _listener = listener;
    }

    public event EventHandler<NotificationArrival>? Added;

    public event EventHandler<uint>? Removed;

    /// <summary>True when Windows pushes changes (package identity); false when polling once per second.</summary>
    public bool IsInstant => _usesEvent;

    public static WindowsNotificationListener Create()
    {
        return new WindowsNotificationListener(UserNotificationListener.Current);
    }

    /// <summary>Asks Windows for notification access (FR-12). True when allowed.</summary>
    public async Task<bool> RequestAccessAsync()
    {
        try
        {
            UserNotificationListenerAccessStatus status = await _listener.RequestAccessAsync();
            return status == UserNotificationListenerAccessStatus.Allowed;
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Asking for notification access failed", error);
            return false;
        }
    }

    /// <summary>Reads whether access is allowed right now, without asking (used for the regular check).</summary>
    public bool IsAccessAllowed()
    {
        try
        {
            return _listener.GetAccessStatus() == UserNotificationListenerAccessStatus.Allowed;
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Reading notification access failed", error);
            return false;
        }
    }

    /// <summary>Takes note of what is already there, then starts reporting changes.</summary>
    public async Task StartAsync()
    {
        foreach (UserNotification existing in await ReadAllAsync())
        {
            _known.Add(existing.Id);
        }

        try
        {
            _listener.NotificationChanged += OnNotificationChanged;
            _usesEvent = true;
        }
        catch (Exception error)
        {
            // Spike S1: 0x80070490 without package identity. Poll instead.
            PlatformLog.Problem("Notification event not available, polling once per second", error);
            _usesEvent = false;
            _ = PollAsync(_stop.Token);
        }
    }

    /// <summary>
    /// FR-36 (v1.4): removes every notification in the Windows notification center that matches,
    /// with the listener's own remove call. Windows then reports each one as removed. Returns how
    /// many were removed; a failure is logged and the rest are still tried.
    /// </summary>
    public async Task<int> RemoveMatchingAsync(Func<Core.Notifications.RawNotification, bool> matches)
    {
        ArgumentNullException.ThrowIfNull(matches);

        int removed = 0;
        foreach (UserNotification notification in await ReadAllAsync())
        {
            if (!matches(ToastReader.Read(notification)))
            {
                continue;
            }

            try
            {
                _listener.RemoveNotification(notification.Id);
                removed++;
            }
            catch (Exception error)
            {
                PlatformLog.Problem("Clearing a notification from Windows failed", error);
            }
        }

        return removed;
    }

    public void Dispose()
    {
        _stop.Cancel();
        if (_usesEvent)
        {
            _listener.NotificationChanged -= OnNotificationChanged;
        }

        _stop.Dispose();
    }

    private void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        _ = HandleChangeAsync(args.ChangeKind, args.UserNotificationId);
    }

    private async Task HandleChangeAsync(UserNotificationChangedKind kind, uint id)
    {
        await _gate.WaitAsync();
        try
        {
            if (kind == UserNotificationChangedKind.Removed)
            {
                if (_known.Remove(id))
                {
                    Removed?.Invoke(this, id);
                }

                return;
            }

            UserNotification? notification = _listener.GetNotification(id);
            if (notification is not null && _known.Add(id))
            {
                await ReportAddedAsync(notification);
            }
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Handling a notification change failed", error);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task PollAsync(CancellationToken stop)
    {
        using PeriodicTimer timer = new(PollInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stop))
            {
                await PollOnceAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Stopping: nothing to do.
        }
    }

    private async Task PollOnceAsync()
    {
        await _gate.WaitAsync();
        try
        {
            IReadOnlyList<UserNotification> current = await ReadAllAsync();
            HashSet<uint> present = [];
            foreach (UserNotification notification in current)
            {
                present.Add(notification.Id);
                if (_known.Add(notification.Id))
                {
                    await ReportAddedAsync(notification);
                }
            }

            foreach (uint gone in _known.Where(id => !present.Contains(id)).ToList())
            {
                _known.Remove(gone);
                Removed?.Invoke(this, gone);
            }
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Checking notifications failed", error);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IReadOnlyList<UserNotification>> ReadAllAsync()
    {
        try
        {
            return await _listener.GetNotificationsAsync(NotificationKinds.Toast);
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Reading notifications failed", error);
            return [];
        }
    }

    private async Task ReportAddedAsync(UserNotification notification)
    {
        Core.Notifications.RawNotification raw = ToastReader.Read(notification);
        byte[]? logo = await LogoForAsync(notification, raw.AppUserModelId);
        Added?.Invoke(this, new NotificationArrival(raw, logo));
    }

    private async Task<byte[]?> LogoForAsync(UserNotification notification, string appId)
    {
        if (_logos.TryGetValue(appId, out byte[]? cached))
        {
            return cached;
        }

        byte[]? logo = null;
        try
        {
            // Some desktop apps (Discord, seen in the v1.0.0 test) have no logo in Windows: null, not an error.
            RandomAccessStreamReference? reference = notification.AppInfo.DisplayInfo.GetLogo(new global::Windows.Foundation.Size(LogoSizePixels, LogoSizePixels));
            if (reference is null)
            {
                _logos[appId] = null;
                return null;
            }

            using IRandomAccessStreamWithContentType stream = await reference.OpenReadAsync();
            if (stream.Size > 0 && stream.Size <= MaxLogoBytes)
            {
                IBuffer buffer = await stream.ReadAsync(new global::Windows.Storage.Streams.Buffer((uint)stream.Size), (uint)stream.Size, InputStreamOptions.None);
                logo = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(buffer);
            }
        }
        catch (Exception error)
        {
            // No logo is fine: the island shows the app's letter instead.
            PlatformLog.Problem("Reading an app logo failed", error);
        }

        _logos[appId] = logo;
        return logo;
    }
}
