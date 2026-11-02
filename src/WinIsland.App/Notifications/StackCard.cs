// StackCard.cs: one chat on the notifications page as a stack, like the iPhone (PRD FR-35,
// DESIGN_SPEC section 11). The card on top is a NotificationCard; behind it the cards of the
// older messages peek out underneath:
//   2 messages: one card behind, #3A3A3C, 8 narrower on each side, radius 14, 4 showing.
//   3 or more:  also a second one, #2E2E31, 16 narrower on each side, radius 12, 4 more showing.
// The peeking cards are part of the same element as the card, so they move and fade with it and
// always stay behind it, also mid-animation (Diva saw a grey box flash in the prototype otherwise).
// Only the part that peeks out is drawn: on the Glass look (FR-37) the card on top is see-through,
// and the cards behind would otherwise show through it. Colors come from IslandLookResources.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.App.Island;

namespace WinIsland.App.Notifications;

internal static class StackCard
{
    public const double PeekDips = 4;

    private const double FirstInsetDips = 8;
    private const double SecondInsetDips = 16;
    private const double PeekTopDips = 12;


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
            stack.Children.Add(Behind(IslandLookResources.SecondBehindBrush, SecondInsetDips, 12, bottomGap: 0));
        }

        stack.Children.Add(Behind(IslandLookResources.FirstBehindBrush, FirstInsetDips, 14, bottomGap: (behind - 1) * PeekDips));
        stack.Children.Add(card);
        return stack;
    }

    // Starts under the card (from 12 down) and reaches past its bottom edge by the peek.
    private static Border Behind(string colorKey, double inset, double radius, double bottomGap)
    {
        Border card = new()
        {
            CornerRadius = new CornerRadius(radius),
            Margin = new Thickness(inset, PeekTopDips, inset, bottomGap),
            IsHitTestVisible = false,
        };
        card.SetResourceReference(Border.BackgroundProperty, colorKey);
        card.SizeChanged += (_, e) =>
            card.Clip = new RectangleGeometry(new Rect(0, Math.Max(0, e.NewSize.Height - PeekDips), e.NewSize.Width, PeekDips));
        Grid.SetRowSpan(card, 2);
        return card;
    }
}
