// ChatView.cs: an opened stack on the notifications page (PRD FR-35, DESIGN_SPEC section 11).
//   Header, 36 high: a back chevron 20 in #A6ABB6, the app icon circle 32, the chat name 14
//   semibold (ellipsis), and "N messages" 11 in #A6ABB6 on the right.
//   Up to 3 message rows, newest first, 6 apart: #1C1C1E, radius 14, 34 high, padding 12 on the
//   left and 22 on the right. Text 13 in #E5E5EA on one line with an ellipsis; the time 11 in
//   #A6ABB6 on the right. Total 156, centered in the 190 high page.
// Clicking anywhere on it goes back to the list; the page view handles that.
// "Hide message text" (FR-17) leaves each row with only its time, like the card has no preview.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

internal static class ChatView
{
    private const double HeaderDips = 36;
    private const double RowDips = 34;
    private const double RowGapDips = 6;
    private const double RowRadius = 14;
    private const double IconDips = 32;
    private const string TimeFormat = "HH:mm";

    private static readonly Brush RowBackground = Frozen(Color.FromRgb(0x1C, 0x1C, 0x1E));
    private static readonly Brush MessageText = Frozen(Color.FromRgb(0xE5, 0xE5, 0xEA));
    private static readonly Brush SecondaryText = Frozen(Color.FromRgb(0xA6, 0xAB, 0xB6));
    private static readonly FontFamily IslandFont = new("Segoe UI Variable Text, Segoe UI");

    /// <summary>The header and rows, as separate elements so they can arrive one after another.</summary>
    public static IReadOnlyList<FrameworkElement> Build(NotificationStack chat, NotificationSettings settings, ImageSource? logo)
    {
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(settings);

        List<FrameworkElement> parts = [Header(chat, logo)];
        foreach (IslandNotification message in chat.Messages)
        {
            FrameworkElement row = Row(settings.PreviewFor(message), message.ReceivedAt);
            row.Margin = new Thickness(0, RowGapDips, 0, 0);
            parts.Add(row);
        }

        return parts;
    }

    private static Grid Header(NotificationStack chat, ImageSource? logo)
    {
        Grid header = new() { Height = HeaderDips, Margin = new Thickness(4, 0, 22, 0) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        header.Children.Add(Text("‹", SecondaryText, 20, column: 0, new Thickness(0, 0, 10, 2)));

        AppIconView icon = new() { Width = IconDips, Height = IconDips, LetterSize = 14, VerticalAlignment = VerticalAlignment.Center };
        icon.Show(chat.Newest, logo);
        Grid.SetColumn(icon, 1);
        header.Children.Add(icon);

        TextBlock name = Text(chat.Newest.Sender, Brushes.White, 14, column: 2, new Thickness(10, 0, 0, 0));
        name.FontWeight = FontWeights.SemiBold;
        name.TextTrimming = TextTrimming.CharacterEllipsis;
        header.Children.Add(name);

        header.Children.Add(Text(chat.CountText, SecondaryText, 11, column: 3, new Thickness(8, 0, 0, 0)));
        return header;
    }

    private static Border Row(string text, DateTimeOffset receivedAt)
    {
        Grid content = new();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        TextBlock message = Text(text, MessageText, 13, column: 0, new Thickness(0));
        message.TextTrimming = TextTrimming.CharacterEllipsis;
        content.Children.Add(message);

        TextBlock time = Text(receivedAt.ToLocalTime().ToString(TimeFormat, CultureInfo.InvariantCulture), SecondaryText, 11, column: 1, new Thickness(8, 0, 0, 0));
        time.Typography.NumeralAlignment = FontNumeralAlignment.Tabular;
        content.Children.Add(time);

        return new Border
        {
            Background = RowBackground,
            CornerRadius = new CornerRadius(RowRadius),
            Height = RowDips,
            Padding = new Thickness(12, 0, 22, 0),
            Child = content,
        };
    }

    private static TextBlock Text(string text, Brush color, double size, int column, Thickness margin)
    {
        TextBlock block = new()
        {
            Text = text,
            Foreground = color,
            FontFamily = IslandFont,
            FontSize = size,
            Margin = margin,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(block, column);
        return block;
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
