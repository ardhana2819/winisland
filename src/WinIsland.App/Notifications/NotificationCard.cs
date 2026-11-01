// NotificationCard.cs: builds one grey notification card for the notifications page
// (PRD FR-26, DESIGN_SPEC section 9).
//   Card #1C1C1E, radius 18, 52 high, padding 12 on the left and 22 on the right (the page dots
//   sit on the right edge, so text keeps clear of them), 10 gap.
//   App icon circle 32 with a 14 letter. Top line: sender 14 semibold, and on the right the time
//   it arrived, 11 in #A6ABB6, 24-hour. Second line: preview 13 in #A6ABB6.
//   A single notification shows up to 3 lines of preview at line height 18 and grows to fit.
// Long text is cut with an ellipsis, so a card never grows past the island.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

internal static class NotificationCard
{
    public const double Height = 52;
    public const double MaxLines = 3;

    private const double Radius = 18;
    private const double IconDips = 32;
    private const double LetterSize = 14;
    private const double GapDips = 10;
    private const double TextLineDips = 18;
    private const string TimeFormat = "HH:mm";

    private static readonly Thickness CardPadding = new(12, 0, 22, 0);
    private static readonly Thickness SingleCardPadding = new(12, 12, 22, 12);
    private static readonly Brush CardBackground = Frozen(Color.FromRgb(0x1C, 0x1C, 0x1E));
    private static readonly Brush SecondaryText = Frozen(Color.FromRgb(0xA6, 0xAB, 0xB6));
    private static readonly FontFamily IslandFont = new("Segoe UI Variable Text, Segoe UI");

    /// <summary>A card; isSingle lets the preview wrap to 3 lines and the card grow to fit.</summary>
    public static FrameworkElement Build(IslandNotification notification, string preview, ImageSource? logo, bool isSingle)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Grid content = new();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        VerticalAlignment alignment = isSingle ? VerticalAlignment.Top : VerticalAlignment.Center;
        AppIconView icon = new() { Width = IconDips, Height = IconDips, LetterSize = LetterSize, VerticalAlignment = alignment };
        icon.Show(notification, logo);
        content.Children.Add(icon);

        StackPanel texts = new() { Margin = new Thickness(GapDips, 0, 0, 0), VerticalAlignment = alignment };
        texts.Children.Add(TopLine(notification));
        if (!string.IsNullOrEmpty(preview))
        {
            texts.Children.Add(PreviewLine(preview, isSingle));
        }

        Grid.SetColumn(texts, 1);
        content.Children.Add(texts);

        Border card = new()
        {
            Background = CardBackground,
            CornerRadius = new CornerRadius(Radius),
            Padding = isSingle ? SingleCardPadding : CardPadding,
            Child = content,
        };
        if (!isSingle)
        {
            card.Height = Height;
        }

        return card;
    }

    private static Grid TopLine(IslandNotification notification)
    {
        Grid line = new();
        line.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        line.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        line.Children.Add(new TextBlock
        {
            Text = notification.Sender,
            Foreground = Brushes.White,
            FontFamily = IslandFont,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });

        TextBlock time = new()
        {
            Text = notification.ReceivedAt.ToLocalTime().ToString(TimeFormat, System.Globalization.CultureInfo.InvariantCulture),
            Foreground = SecondaryText,
            FontFamily = IslandFont,
            FontSize = 11,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        time.Typography.NumeralAlignment = FontNumeralAlignment.Tabular;
        Grid.SetColumn(time, 1);
        line.Children.Add(time);
        return line;
    }

    private static TextBlock PreviewLine(string preview, bool isSingle)
    {
        TextBlock block = new()
        {
            Text = preview,
            Foreground = SecondaryText,
            FontFamily = IslandFont,
            FontSize = 13,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        if (isSingle)
        {
            block.TextWrapping = TextWrapping.Wrap;
            block.LineHeight = TextLineDips;
            block.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            block.MaxHeight = MaxLines * TextLineDips;
        }

        return block;
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
