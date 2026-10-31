// NotificationListView.xaml.cs: builds the rows of the last 3 list and works out its height.
// With one notification it shows the whole message, up to 3 lines (FR-15). The number of lines is
// measured from the real wrapped text, not estimated from the character count like the
// prototype (Diva confirmed this on 31 Oct 2026), so the island's height fits the text.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class NotificationListView : UserControl
{
    private const double PaddingTotalDips = 24;
    private const double RowDips = 50;
    private const double RowGapDips = 8;
    private const double SenderLineDips = 22;
    private const double TextLineDips = 18;
    private const double SingleBottomDips = 6;
    private const int MaxLines = 3;
    private const double IconDips = 36;
    private const double IconGapDips = 12;
    private const double InnerWidthDips = 420 - 32;

    private static readonly Brush SecondaryText = Frozen(Color.FromRgb(0xA6, 0xAB, 0xB6));
    private static readonly FontFamily IslandFont = new("Segoe UI Variable Text, Segoe UI");

    public NotificationListView()
    {
        InitializeComponent();
    }

    /// <summary>The island height this list needs (DESIGN_SPEC section 2).</summary>
    public double RequiredHeight { get; private set; } = PaddingTotalDips + RowDips;

    public void Show(IReadOnlyList<IslandNotification> notifications, NotificationSettings settings, Func<IslandNotification, ImageSource?> logoFor)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logoFor);

        Rows.Children.Clear();
        if (notifications.Count == 1)
        {
            ShowSingle(notifications[0], settings.PreviewFor(notifications[0]), logoFor(notifications[0]));
            return;
        }

        for (int index = 0; index < notifications.Count; index++)
        {
            IslandNotification notification = notifications[index];
            FrameworkElement row = BuildRow(notification, settings.PreviewFor(notification), logoFor(notification), wrapAllLines: false);
            row.Height = RowDips;
            row.Margin = new Thickness(0, index == 0 ? 0 : RowGapDips, 0, 0);
            Rows.Children.Add(row);
        }

        int count = Math.Max(1, notifications.Count);
        RequiredHeight = PaddingTotalDips + (count * RowDips) + ((count - 1) * RowGapDips);
    }

    private void ShowSingle(IslandNotification notification, string preview, ImageSource? logo)
    {
        Rows.Children.Add(BuildRow(notification, preview, logo, wrapAllLines: true));
        int lines = string.IsNullOrEmpty(preview) ? 1 : MeasureLines(preview);
        RequiredHeight = PaddingTotalDips + SenderLineDips + (lines * TextLineDips) + SingleBottomDips;
    }

    private static FrameworkElement BuildRow(IslandNotification notification, string preview, ImageSource? logo, bool wrapAllLines)
    {
        Grid row = new();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        AppIconView icon = new()
        {
            Width = IconDips,
            Height = IconDips,
            VerticalAlignment = wrapAllLines ? VerticalAlignment.Top : VerticalAlignment.Center,
        };
        icon.Show(notification, logo);
        row.Children.Add(icon);

        StackPanel texts = new()
        {
            Margin = new Thickness(IconGapDips, 0, 0, 0),
            VerticalAlignment = wrapAllLines ? VerticalAlignment.Top : VerticalAlignment.Center,
        };
        texts.Children.Add(new TextBlock
        {
            Text = notification.Sender,
            Foreground = Brushes.White,
            FontFamily = IslandFont,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });

        if (!string.IsNullOrEmpty(preview))
        {
            texts.Children.Add(PreviewBlock(preview, wrapAllLines));
        }

        Grid.SetColumn(texts, 1);
        row.Children.Add(texts);
        return row;
    }

    private static TextBlock PreviewBlock(string preview, bool wrapAllLines)
    {
        TextBlock block = new()
        {
            Text = preview,
            Foreground = SecondaryText,
            FontFamily = IslandFont,
            FontSize = 13,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        if (wrapAllLines)
        {
            block.TextWrapping = TextWrapping.Wrap;
            block.LineHeight = TextLineDips;
            block.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            block.MaxHeight = MaxLines * TextLineDips;
        }

        return block;
    }

    private static int MeasureLines(string preview)
    {
        TextBlock probe = PreviewBlock(preview, wrapAllLines: true);
        probe.MaxHeight = double.PositiveInfinity;
        probe.Measure(new Size(InnerWidthDips - IconDips - IconGapDips, double.PositiveInfinity));
        int lines = (int)Math.Ceiling(probe.DesiredSize.Height / TextLineDips);
        return Math.Clamp(lines, 1, MaxLines);
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
