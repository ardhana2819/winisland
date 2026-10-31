// AppIconView.xaml.cs: shows the app logo, or the colored letter circle when there is none.
// The design names colors for WhatsApp and Discord only; other apps use the design's dark grey
// (#3A3A3C, the progress track color) so they stay quiet next to the two main apps.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class AppIconView : UserControl
{
    private static readonly Brush WhatsAppGreen = Frozen(Color.FromRgb(0x17, 0x7A, 0x40));
    private static readonly Brush DiscordBlue = Frozen(Color.FromRgb(0x47, 0x52, 0xC4));
    private static readonly Brush OtherGrey = Frozen(Color.FromRgb(0x3A, 0x3A, 0x3C));

    public AppIconView()
    {
        InitializeComponent();
    }

    public void Show(IslandNotification notification, ImageSource? logo)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logo is not null)
        {
            ImageBrush picture = new(logo) { Stretch = Stretch.UniformToFill };
            picture.Freeze();
            Circle.Fill = picture;
            Letter.Visibility = Visibility.Collapsed;
            return;
        }

        Circle.Fill = notification.Source switch
        {
            NotificationSource.WhatsApp => WhatsAppGreen,
            NotificationSource.Discord => DiscordBlue,
            _ => OtherGrey,
        };
        Letter.Text = notification.AppName.Length > 0 ? notification.AppName[..1].ToUpperInvariant() : "?";
        Letter.Visibility = Visibility.Visible;
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
