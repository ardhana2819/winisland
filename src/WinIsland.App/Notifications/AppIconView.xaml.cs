// AppIconView.xaml.cs: shows the app icon on banners and cards.
//   WhatsApp (desktop and Web): always the green circle with the white WhatsApp logo, drawn by
//   WinIsland (Diva, 1 Nov 2026), so it looks the same whatever Windows sends.
//   Discord: always the blurple circle with the white Discord logo, the same way (PRD FR-30).
//   Snipping Tool: always the purple circle with white scissors, as in the prototype (Diva, 3 Nov
//   2026), because its own logo looked broken.
//   Other apps (v1.5, FR-42, Diva 3 Nov 2026): like WhatsApp and Discord, the logo in white on the
//   app's own colour (AppLogoStyles), for example Claude's white mark on orange; a logo that is a
//   full tile fills the circle as it is; no logo gives the grey letter circle (#3A3A3C).
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
    // v1.4 (Diva, 2 Nov 2026): brighter, the same green as the call symbol; was #177A40.
    private static readonly Brush WhatsAppGreen = Frozen(Color.FromRgb(0x30, 0xD1, 0x58));
    private static readonly Brush DiscordBlurple = Frozen(Color.FromRgb(0x58, 0x65, 0xF2));
    private static readonly Brush SnippingToolPurple = Frozen(Color.FromRgb(0x6A, 0x5A, 0xCD));
    private static readonly Brush OtherGrey = Frozen(Color.FromRgb(0x3A, 0x3A, 0x3C));
    // The logo takes 58 percent of the circle, like the WhatsApp and Discord logos.
    private const double LogoShare = 0.58;
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

        double size = double.IsNaN(Width) ? ActualWidth : Width;
        LogoMark.Visibility = Visibility.Collapsed;
        Geometry? drawnLogo = AppLogoGlyph.ForCircle(notification, size);
        if (drawnLogo is not null)
        {
            Circle.Fill = notification.Source switch
            {
                NotificationSource.Discord => DiscordBlurple,
                NotificationSource.WhatsApp => WhatsAppGreen,
                _ => SnippingToolPurple,
            };
            Letter.Data = drawnLogo;
            Letter.Visibility = Visibility.Visible;
            return;
        }

        AppLogoStyle? style = logo is null ? null : AppLogoStyles.For(logo);
        if (logo is not null && style is { IsShape: true })
        {
            // Only the logo's visible part, so empty margins never shrink it (FR-42).
            ImageBrush shape = new(logo) { Stretch = Stretch.Uniform, Viewbox = VisiblePart(style), ViewboxUnits = BrushMappingMode.RelativeToBoundingBox };
            shape.Freeze();
            Circle.Fill = Frozen(Color.FromRgb(style.Circle.R, style.Circle.G, style.Circle.B));
            LogoMark.Width = size * LogoShare;
            LogoMark.Height = size * LogoShare;
            LogoMark.OpacityMask = shape;
            LogoMark.Visibility = Visibility.Visible;
            Letter.Visibility = Visibility.Collapsed;
            return;
        }

        if (logo is not null)
        {
            // A plate: its visible part fills the circle, centered (UniformToFill).
            ImageBrush picture = new(logo) { Stretch = Stretch.UniformToFill };
            if (style is not null)
            {
                picture.Viewbox = VisiblePart(style);
                picture.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
            }

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

    private static Rect VisiblePart(AppLogoStyle style) => new(style.Visible.Left, style.Visible.Top, style.Visible.Width, style.Visible.Height);

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
