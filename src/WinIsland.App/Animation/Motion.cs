// Motion.cs: small helpers so every island animation is built the same way, with the design's
// curves and timings, and so all of them respect the Windows "Animation effects" setting
// (DESIGN_SPEC section 6, reduced motion: when it is off, states change instantly).

using System.Windows;
using System.Windows.Media.Animation;

namespace WinIsland.App.Animation;

internal static class Motion
{
    /// <summary>False when Windows Settings > Accessibility > Visual effects > Animation effects is off.</summary>
    public static bool IsEnabled => SystemParameters.ClientAreaAnimation;

    public static DoubleAnimation To(double target, double seconds, IEasingFunction easing, double delaySeconds = 0)
    {
        DoubleAnimation animation = new(target, TimeSpan.FromSeconds(seconds))
        {
            EasingFunction = easing,
            BeginTime = TimeSpan.FromSeconds(delaySeconds),
        };
        animation.Freeze();
        return animation;
    }

    /// <summary>Animates a property, or sets it at once when animations are off.</summary>
    public static void Run(IAnimatable target, DependencyProperty property, DoubleAnimation animation)
    {
        if (IsEnabled)
        {
            target.BeginAnimation(property, animation);
            return;
        }

        // Stop any running animation, then jump straight to the end value.
        target.BeginAnimation(property, null);
        if (target is DependencyObject dependencyObject && animation.To is double end)
        {
            dependencyObject.SetValue(property, end);
        }
    }
}
