// AppIconView.xaml.cs: shows the app icon on banners and cards.
//   WhatsApp (desktop and Web): always the green circle with the white WhatsApp logo, drawn by
//   WinIsland (Diva, 1 Nov 2026), so it looks the same whatever Windows sends.
//   Discord: always the blurple circle with the white Discord logo, the same way (PRD FR-30).
//   Other apps: the app logo from the notification, or the grey letter circle when there is none.
// Other apps use the design's dark grey (#3A3A3C, the progress track color) so they stay quiet
// next to the two main apps.
// The letter is turned into a shape and moved so its ink starts at (0,0); the Path is then
// centered in the circle by that ink, which puts the letter exactly in the middle.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class AppIconView : UserControl
{
    private static readonly Brush WhatsAppGreen = Frozen(Color.FromRgb(0x17, 0x7A, 0x40));
    private static readonly Brush DiscordBlurple = Frozen(Color.FromRgb(0x58, 0x65, 0xF2));
    private static readonly Brush OtherGrey = Frozen(Color.FromRgb(0x3A, 0x3A, 0x3C));
    private static readonly Typeface LetterFont = new(new FontFamily("Segoe UI Variable Text, Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

    public AppIconView()
    {
        InitializeComponent();
    }

    /// <summary>The letter's font size: 16 on the banner, 14 on cards (DESIGN_SPEC section 9).</summary>
    public double LetterSize { get; set; } = 16;

    public void Show(IslandNotification notification, ImageSource? logo)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Geometry? drawnLogo = AppLogoGlyph.ForCircle(notification.Source, double.IsNaN(Width) ? ActualWidth : Width);
        if (drawnLogo is not null)
        {
            Circle.Fill = notification.Source == NotificationSource.Discord ? DiscordBlurple : WhatsAppGreen;
            Letter.Data = drawnLogo;
            Letter.Visibility = Visibility.Visible;
            return;
        }

        if (logo is not null)
        {
            // UniformToFill keeps the logo centered and fills the circle.
            ImageBrush picture = new(logo) { Stretch = Stretch.UniformToFill };
            picture.Freeze();
            Circle.Fill = picture;
            Letter.Visibility = Visibility.Collapsed;
            return;
        }

        Circle.Fill = OtherGrey;
        string letter = notification.AppName.Length > 0 ? notification.AppName[..1].ToUpperInvariant() : "?";
        Letter.Data = CenteredLetter(letter, LetterSize);
        Letter.Visibility = Visibility.Visible;
    }

    private static Geometry CenteredLetter(string letter, double size)
    {
        FormattedText text = new(letter, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, LetterFont, size, Brushes.White, 1.0);
        Geometry shape = text.BuildGeometry(new Point(0, 0));
        Rect ink = shape.Bounds;
        if (ink.IsEmpty)
        {
            return Geometry.Empty;
        }

        // Move the ink to (0,0), so the Path's size is exactly the letter and centering is exact.
        shape.Transform = new TranslateTransform(-ink.X, -ink.Y);
        shape.Freeze();
        return shape;
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
