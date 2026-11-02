// GlassLook.cs: turns the Glass look on and off and keeps the blur exactly under the island
// (PRD FR-37, DESIGN_SPEC section 11).
// On: a BlurWindow is placed over the island window and made the island's owner, which keeps the
// island always above it. Its blur follows the pill's place, size, corners and opacity, so it
// moves with every shape change, the hide animation and the size change in the same frame
// (glass spike G3).
// Following costs a little each frame, so it runs only while something may be moving: it starts
// when the island changes or lays out, and stops once the pill has been still for half a second.
// If Windows refuses the blur, Enable returns false and logs why; the island then shows Solid and
// tries Glass again on the next start.

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinIsland.App.Island;

internal sealed class GlassLook : IDisposable
{
    private const int StillFramesBeforeStop = 30;

    private readonly Window _island;
    private readonly Border _pill;
    private BlurWindow? _blur;
    private bool _isFollowing;
    private int _stillFrames;
    private (int Left, int Top, int Width, int Height, int Radius, float Opacity) _shown;

    public GlassLook(Window island, Border pill)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(pill);
        _island = island;
        _pill = pill;
        _pill.LayoutUpdated += (_, _) => Follow();
    }

    public bool IsOn => _blur?.Backdrop is not null;

    /// <summary>Starts the blur. Returns false when Windows refused it (the caller shows Solid).</summary>
    public bool Enable()
    {
        if (IsOn)
        {
            return true;
        }

        BlurWindow blur = new() { Left = _island.Left, Top = _island.Top, Width = _island.Width, Height = _island.Height };
        blur.Show();
        if (blur.Backdrop is null)
        {
            Trace.TraceWarning("Glass look unavailable, showing Solid: {0} (HRESULT 0x{1:X8})", blur.Problem?.GetType().Name, blur.Problem?.HResult);
            blur.Close();
            return false;
        }

        _blur = blur;
        _island.Owner = blur;
        _shown = default;
        Follow();
        return true;
    }

    public void Disable()
    {
        if (_blur is null)
        {
            return;
        }

        StopFollowing();
        _island.Owner = null;
        _blur.Close();
        _blur = null;
    }

    /// <summary>Something may move: follow the pill until it is still again.</summary>
    public void Follow()
    {
        _stillFrames = 0;
        if (_isFollowing || !IsOn)
        {
            return;
        }

        _isFollowing = true;
        CompositionTarget.Rendering += OnRendering;
    }

    // Called when the island window has closed: its owner can no longer be changed, only the blur
    // window closes.
    public void Dispose()
    {
        StopFollowing();
        _blur?.Close();
        _blur = null;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_blur?.Backdrop is null)
        {
            StopFollowing();
            return;
        }

        PlaceOverIsland(_blur);
        (int, int, int, int, int, float) pill = PillInPixels();
        if (pill == _shown)
        {
            if (++_stillFrames >= StillFramesBeforeStop)
            {
                StopFollowing();
            }

            return;
        }

        _stillFrames = 0;
        _shown = pill;
        (int left, int top, int width, int height, int radius, float opacity) = pill;
        _blur.Backdrop.Update(left, top, width, height, radius, opacity);
    }

    // The island window never moves while the island changes size; it can move with the display.
    private void PlaceOverIsland(Window blur)
    {
        if (blur.Left != _island.Left || blur.Top != _island.Top || blur.Width != _island.Width || blur.Height != _island.Height)
        {
            blur.Left = _island.Left;
            blur.Top = _island.Top;
            blur.Width = _island.Width;
            blur.Height = _island.Height;
        }
    }

    // The pill as it is drawn: all its scales and moves applied, in the window's pixels.
    private (int Left, int Top, int Width, int Height, int Radius, float Opacity) PillInPixels()
    {
        if (_pill.ActualWidth <= 0 || !_pill.IsVisible)
        {
            return (0, 0, 0, 0, 0, 0);
        }

        Rect bounds = _pill.TransformToAncestor(_island).TransformBounds(new Rect(0, 0, _pill.ActualWidth, _pill.ActualHeight));
        DpiScale dpi = VisualTreeHelper.GetDpi(_island);
        double scale = bounds.Width / _pill.ActualWidth;
        int radius = (int)Math.Round(PillGeometry.GetRadius(_pill) * scale * dpi.DpiScaleX);
        return (
            (int)Math.Round(bounds.Left * dpi.DpiScaleX),
            (int)Math.Round(bounds.Top * dpi.DpiScaleY),
            (int)Math.Round(bounds.Width * dpi.DpiScaleX),
            (int)Math.Round(bounds.Height * dpi.DpiScaleY),
            radius,
            (float)Math.Round(_pill.Opacity, 2));
    }

    private void StopFollowing()
    {
        if (!_isFollowing)
        {
            return;
        }

        _isFollowing = false;
        CompositionTarget.Rendering -= OnRendering;
    }
}
