// PageScroll.cs: turns mouse wheel and touchpad scrolling into page changes (PRD FR-21).
// One gesture moves one page. A touchpad sends many small scroll steps for one swipe, and a mouse
// wheel sends one step per notch; steps less than 300 ms apart belong to the same gesture, so only
// the first step of a gesture counts (DESIGN_SPEC section 9). Scrolling down (away from you on a
// touchpad, towards you on a wheel) goes to notifications, up goes to music.

namespace WinIsland.Core.Island;

public sealed class PageScroll
{
    public static readonly TimeSpan GestureGap = TimeSpan.FromMilliseconds(300);

    private DateTimeOffset? _lastStepAt;

    /// <summary>
    /// One scroll step. delta follows Windows: positive scrolls up, negative scrolls down.
    /// Returns the page to show, or null when the step is part of a gesture that already counted.
    /// </summary>
    public ExpandedPage? Step(int delta, DateTimeOffset at)
    {
        bool startsGesture = _lastStepAt is not DateTimeOffset last || at - last >= GestureGap;
        _lastStepAt = at;

        if (!startsGesture || delta == 0)
        {
            return null;
        }

        return delta < 0 ? ExpandedPage.Notifications : ExpandedPage.Music;
    }
}
