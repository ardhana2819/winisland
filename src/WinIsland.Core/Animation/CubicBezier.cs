// CubicBezier.cs: the timing curves from the design, as math.
// DESIGN_SPEC section 6 uses CSS cubic-bezier curves. The spring curve (0.34, 1.4, 0.56, 1)
// overshoots past 1 and settles, which WPF's built-in KeySpline cannot express, so the curve is
// calculated here and the app wraps it as a WPF easing function. Same numbers, same motion.

namespace WinIsland.Core.Animation;

public sealed class CubicBezier
{
    private const int NewtonSteps = 8;
    private const int BisectionSteps = 30;
    private const double Tolerance = 1e-7;

    private readonly double _x1;
    private readonly double _y1;
    private readonly double _x2;
    private readonly double _y2;

    public CubicBezier(double x1, double y1, double x2, double y2)
    {
        // The x values are time, so they must stay between 0 and 1 (same rule as CSS).
        if (x1 is < 0 or > 1 || x2 is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(x1), "The x values of a timing curve must be between 0 and 1.");
        }

        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
    }

    /// <summary>The island's spring-like curve: overshoots slightly, then settles (DESIGN_SPEC section 6).</summary>
    public static CubicBezier Spring { get; } = new(0.34, 1.4, 0.56, 1);

    /// <summary>The CSS default "ease" curve, used by the prototype for fades without a named curve.</summary>
    public static CubicBezier Ease { get; } = new(0.25, 0.1, 0.25, 1);

    /// <summary>Returns how far along the motion is (can go above 1 for the spring) at a point in time from 0 to 1.</summary>
    public double Evaluate(double timeProgress)
    {
        if (timeProgress <= 0)
        {
            return 0;
        }

        if (timeProgress >= 1)
        {
            return 1;
        }

        double curveParameter = SolveCurveParameterForX(timeProgress);
        return SampleCurve(_y1, _y2, curveParameter);
    }

    private double SolveCurveParameterForX(double x)
    {
        // Newton's method is fast and almost always enough.
        double parameter = x;
        for (int step = 0; step < NewtonSteps; step++)
        {
            double error = SampleCurve(_x1, _x2, parameter) - x;
            if (Math.Abs(error) < Tolerance)
            {
                return parameter;
            }

            double slope = SampleSlope(_x1, _x2, parameter);
            if (Math.Abs(slope) < Tolerance)
            {
                break;
            }

            parameter -= error / slope;
        }

        // Fall back to bisection, which always finds the answer because x grows steadily with the parameter.
        double low = 0;
        double high = 1;
        parameter = x;
        for (int step = 0; step < BisectionSteps; step++)
        {
            double value = SampleCurve(_x1, _x2, parameter);
            if (Math.Abs(value - x) < Tolerance)
            {
                return parameter;
            }

            if (value < x)
            {
                low = parameter;
            }
            else
            {
                high = parameter;
            }

            parameter = (low + high) / 2;
        }

        return parameter;
    }

    // Cubic Bezier with fixed end points (0,0) and (1,1), for one axis.
    private static double SampleCurve(double point1, double point2, double parameter)
    {
        double inverse = 1 - parameter;
        return (3 * inverse * inverse * parameter * point1) + (3 * inverse * parameter * parameter * point2) + (parameter * parameter * parameter);
    }

    private static double SampleSlope(double point1, double point2, double parameter)
    {
        double inverse = 1 - parameter;
        return (3 * inverse * inverse * point1) + (6 * inverse * parameter * (point2 - point1)) + (3 * parameter * parameter * (1 - point2));
    }
}
