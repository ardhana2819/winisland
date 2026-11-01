// IslandNotificationPresenter.cs: keeps the notification views on the island up to date.
// Fills the banner (newest waiting notification and the +N badge) and the notifications page
// (the last 3 on cards, or "No notifications") from the island's state, with "Hide message text"
// applied (FR-17), and passes tap and swipe on the banner back to the island's rules (FR-15, FR-22).

using WinIsland.App.Notifications;
using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.App.Island;

internal sealed class IslandNotificationPresenter
{
    private readonly IslandStateMachine _island;
    private readonly NotificationController _notifications;
    private readonly NotificationBannerView _banner;
    private readonly NotificationsPageView _page;

    public IslandNotificationPresenter(
        IslandStateMachine island,
        NotificationController notifications,
        NotificationBannerView banner,
        NotificationsPageView page)
    {
        _island = island;
        _notifications = notifications;
        _banner = banner;
        _page = page;

        _banner.Tapped += (_, _) => _island.OpenNotificationsPage();
        _banner.Swiped += (_, _) => _island.SwipeBanner(DateTimeOffset.Now);
    }

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

        _page.Show(_island.RecentNotifications, settings, _notifications.LogoFor);
    }
}
