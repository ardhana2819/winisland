// NotificationsPageView.xaml.cs: fills the notifications page with cards (PRD FR-22, FR-26).
// "Hide message text" is applied by the caller through NotificationSettings (FR-17).
// Clicks on a card are swallowed, so they neither open an app nor close the island (FR-22).

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class NotificationsPageView : UserControl
{
    private const double CardGapDips = 6;
    private const double SingleTopDips = 10;

    public NotificationsPageView()
    {
        InitializeComponent();
    }

    public void Show(IReadOnlyList<IslandNotification> notifications, NotificationSettings settings, Func<IslandNotification, ImageSource?> logoFor)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logoFor);

        Cards.Children.Clear();
        EmptyText.Visibility = notifications.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        bool isSingle = notifications.Count == 1;
        Cards.VerticalAlignment = isSingle ? VerticalAlignment.Top : VerticalAlignment.Center;
        Cards.Margin = new Thickness(10, isSingle ? SingleTopDips : 0, 10, 0);

        for (int index = 0; index < notifications.Count; index++)
        {
            IslandNotification notification = notifications[index];
            FrameworkElement card = NotificationCard.Build(notification, settings.PreviewFor(notification), logoFor(notification), isSingle);
            card.Margin = new Thickness(0, index == 0 ? 0 : CardGapDips, 0, 0);
            card.MouseLeftButtonUp += SwallowClick;
            Cards.Children.Add(card);
        }
    }

    private static void SwallowClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
