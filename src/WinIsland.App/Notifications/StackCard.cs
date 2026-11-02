// StackCard.cs: one chat on the notifications page as a stack, like the iPhone (PRD FR-35,
// DESIGN_SPEC section 11). The card on top is a NotificationCard; behind it the cards of the
// older messages peek out underneath:
//   2 messages: one card behind, #3A3A3C, 8 narrower on each side, radius 14, 4 showing.
//   3 or more:  also a second one, #2E2E31, 16 narrower on each side, radius 12, 4 more showing.
// The peeking cards are part of the same element as the card, so they move and fade with it and
// always stay behind it, also mid-animation (Diva saw a grey box flash in the prototype otherwise).

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinIsland.App.Notifications;

internal static class StackCard
{
    public const double PeekDips = 4;

    private const double FirstInsetDips = 8;
    private const double SecondInsetDips = 16;
    private const double PeekTopDips = 12;

    private static readonly Brush FirstBehind = Frozen(Color.FromRgb(0x3A, 0x3A, 0x3C));
    private static readonly Brush SecondBehind = Frozen(Color.FromRgb(0x2E, 0x2E, 0x31));

    /// <summary>Wraps a card; messageCount 1 returns it with no cards behind.</summary>
    public static FrameworkElement Wrap(FrameworkElement card, int messageCount)
    {
        ArgumentNullException.ThrowIfNull(card);

        int behind = Math.Clamp(messageCount - 1, 0, 2);
        if (behind == 0)
        {
            return card;
        }

        Grid stack = new();
        stack.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        stack.RowDefinitions.Add(new RowDefinition { Height = new GridLength(behind * PeekDips) });

        if (behind == 2)
        {
            stack.Children.Add(Behind(SecondBehind, SecondInsetDips, 12, bottomGap: 0));
        }

        stack.Children.Add(Behind(FirstBehind, FirstInsetDips, 14, bottomGap: (behind - 1) * PeekDips));
        stack.Children.Add(card);
        return stack;
    }

    // Starts under the card (from 12 down) and reaches past its bottom edge by the peek.
    private static Border Behind(Brush color, double inset, double radius, double bottomGap)
    {
        Border card = new()
        {
            Background = color,
            CornerRadius = new CornerRadius(radius),
            Margin = new Thickness(inset, PeekTopDips, inset, bottomGap),
            IsHitTestVisible = false,
        };
        Grid.SetRowSpan(card, 2);
        return card;
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
