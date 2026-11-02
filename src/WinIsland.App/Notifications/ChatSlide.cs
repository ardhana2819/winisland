// ChatSlide.cs: the move between the notifications list and an opened chat (PRD FR-35,
// DESIGN_SPEC section 11), like opening a chat on the iPhone:
//   Opening: the list slides 24 to the left while fading out over 0.15 s (ease-in). Then the chat
//   slides in from 40 on the right over 0.5 s with the page-switch spring, fully visible halfway,
//   and its header and rows follow, each from 16 to the right over 0.35 s, 0.04 s apart.
//   Going back is the mirror image.
// With Windows animation effects off the content changes at once (DESIGN_SPEC section 6).
// New content that arrives mid-move waits for the swap, so the move is never cut off halfway.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;

namespace WinIsland.App.Notifications;

internal sealed class ChatSlide
{
    private const double OutSeconds = 0.15;
    private const double OutDips = 24;
    private const double InSeconds = 0.5;
    private const double InFadeSeconds = 0.25;
    private const double InDips = 40;
    private const double PartSeconds = 0.35;
    private const double PartDips = 16;
    private const double PartFirstDelaySeconds = 0.06;
    private const double PartStepSeconds = 0.04;

    private readonly Panel _content;
    private readonly TranslateTransform _shift = new();
    private Action? _pendingFill;

    public ChatSlide(Panel content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
        _content.RenderTransform = _shift;
    }

    /// <summary>True from the start of a move until the new content is in place.</summary>
    public bool IsSwapping => _pendingFill is not null;

    /// <summary>Moves to new content: opening true for list to chat, false for chat to list.</summary>
    public void Run(bool opening, Action fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        if (!Motion.IsEnabled)
        {
            fill();
            return;
        }

        bool alreadyMoving = IsSwapping;
        _pendingFill = fill;
        if (alreadyMoving)
        {
            return;
        }

        double direction = opening ? 1 : -1;
        DoubleAnimation fadeOut = Motion.To(0, OutSeconds, new PowerEase { EasingMode = EasingMode.EaseIn });
        AnimationClock outClock = fadeOut.CreateClock();
        outClock.Completed += (_, _) => SwapIn(direction);
        _content.ApplyAnimationClock(UIElement.OpacityProperty, outClock);
        _shift.BeginAnimation(TranslateTransform.XProperty, Motion.To(-OutDips * direction, OutSeconds, new PowerEase { EasingMode = EasingMode.EaseIn }));
    }

    /// <summary>New content for the same view: placed now, or at the swap when a move is running.</summary>
    public void Refill(Action fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        if (IsSwapping)
        {
            _pendingFill = fill;
            return;
        }

        fill();
    }

    private void SwapIn(double direction)
    {
        Action? fill = _pendingFill;
        _pendingFill = null;
        fill?.Invoke();

        _shift.BeginAnimation(TranslateTransform.XProperty, null);
        _shift.X = InDips * direction;
        _shift.BeginAnimation(TranslateTransform.XProperty, Motion.To(0, InSeconds, CubicBezierEase.PageSlide()));
        _content.BeginAnimation(UIElement.OpacityProperty, null);
        _content.Opacity = 0;
        _content.BeginAnimation(UIElement.OpacityProperty, Motion.To(1, InFadeSeconds, CubicBezierEase.Ease()));

        int index = 0;
        foreach (UIElement part in _content.Children)
        {
            double delay = PartFirstDelaySeconds + (index * PartStepSeconds);
            TranslateTransform partShift = new(PartDips * direction, 0);
            part.RenderTransform = partShift;
            part.Opacity = 0;
            partShift.BeginAnimation(TranslateTransform.XProperty, Motion.To(0, PartSeconds, CubicBezierEase.PageSlide(), delay));
            part.BeginAnimation(UIElement.OpacityProperty, Motion.To(1, PartSeconds, CubicBezierEase.Ease(), delay));
            index++;
        }
    }
}
