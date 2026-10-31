// IslandNotificationPresenter.cs: keeps the notification views on the island up to date.
// Fills the banner (newest waiting notification and the +N badge) and the last 3 list from the
// island's state, with "Hide message text" applied (FR-17), and passes tap and swipe back to the
// island's rules (FR-15).

using WinIsland.App.Notifications;
using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.App.Island;

internal sealed class IslandNotificationPresenter
{
    private readonly IslandStateMachine _island;
    private readonly NotificationController _notifications;
    private readonly NotificationBannerView _banner;
    private readonly NotificationListView _list;

    public IslandNotificationPresenter(
        IslandStateMachine island,
        NotificationController notifications,
        NotificationBannerView banner,
        NotificationListView list)
    {
        _island = island;
        _notifications = notifications;
        _banner = banner;
        _list = list;

        _banner.Tapped += (_, _) => _island.OpenNotificationList();
        _banner.Swiped += (_, _) => _island.SwipeBanner(DateTimeOffset.Now);
    }

    /// <summary>The island height the list needs right now.</summary>
    public double ListHeight => _list.RequiredHeight;

    /// <summary>Call after every island change.</summary>
    public void Refresh()
    {
        NotificationSettings settings = _island.NotificationSettings;

        IReadOnlyList<IslandNotification> waiting = _island.WaitingBanners;
        if (waiting.Count > 0)
        {
            IslandNotification newest = waiting[0];
            _banner.Show(newest, settings.PreviewFor(newest), waiting.Count, _notifications.LogoFor(newest));
        }

        if (_island.RecentNotifications.Count > 0)
        {
            _list.Show(_island.RecentNotifications, settings, _notifications.LogoFor);
        }
    }
}
