// PillGeometry.cs: keeps the pill's rounded corners and its clipping in step while it animates.
// WPF cannot animate a CornerRadius directly, so the radius is an animatable number here, and
// every change updates both the corners and the clip. The clip hides content that would stick out
// past the rounded corners mid-animation, like "overflow: hidden" in the prototype.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinIsland.App.Island;

internal static class PillGeometry
{
    public static readonly DependencyProperty RadiusProperty = DependencyProperty.RegisterAttached(
        "Radius", typeof(double), typeof(PillGeometry), new PropertyMetadata(0.0, OnRadiusChanged));

    public static void SetRadius(Border pill, double radius) => pill.SetValue(RadiusProperty, radius);

    public static double GetRadius(Border pill) => (double)pill.GetValue(RadiusProperty);

    /// <summary>Starts keeping the clip in step with the pill's size.</summary>
    public static void Attach(Border pill)
    {
        pill.SizeChanged += (_, _) => UpdateClip(pill);
        UpdateClip(pill);
    }

    private static void OnRadiusChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is Border pill)
        {
            double radius = (double)e.NewValue;
            pill.CornerRadius = new CornerRadius(radius);
            UpdateClip(pill);
        }
    }

    private static void UpdateClip(Border pill)
    {
        double radius = GetRadius(pill);
        pill.Clip = new RectangleGeometry(new Rect(0, 0, pill.ActualWidth, pill.ActualHeight), radius, radius);
    }
}
