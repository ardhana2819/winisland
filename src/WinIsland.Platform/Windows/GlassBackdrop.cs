// GlassBackdrop.cs: puts Windows' own blur into the glass window, in the island's shape (PRD FR-37).
// This is the way the v1.4 glass spike proved after 10 rounds (docs/spike-results.md):
//   - The blur is Windows.UI.Composition's host backdrop brush (documented). Windows draws it and
//     keeps drawing it while the window is not active, which the island never is.
//   - It lives in an empty window of its own behind the island (GlassWindow), because in a window
//     with WPF content it hides that content (round 7).
//   - It is clipped to a rounded rectangle with smooth edges, exactly the island's shape.
// v1.5 polish (Diva, 4 Nov 2026: the edges looked rough): the glass window is now see-through
// outside the blur, so the smooth clip makes the edge. The window's hard region (no edge smoothing)
// only keeps the window small for clicks and lies a few pixels outside, where nothing shows. Before,
// the region was the edge, to hide the black around the blur (round 10).
// Windows draws the blur; nothing here captures or reads the screen (PRD section 7).
// Two fixes from Diva's first test (2 Nov 2026):
//   - PlaceBelow puts the blur window right under the island; shown later, it had covered the
//     island until something reordered the windows, so the island looked empty.
//   - RefreshBackdrop asks for a fresh blur. After a game (Bleach) was minimized, Windows kept
//     showing its last dark frame in the blur until another app was clicked.

using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;

using Windows.UI.Composition;
using Windows.UI.Composition.Desktop;

using WinRT;

namespace WinIsland.Platform.Windows;

public sealed class GlassBackdrop
{
    // ICompositorDesktopInterop, from windows.ui.composition.interop.h.
    private static readonly Guid CompositorDesktopInteropId = new("29E691FA-4567-4DCA-B319-D0F207EB6807");
    // The hard region lies this far outside the pill, so its stair steps never touch the blur.
    private const int RegionMarginPixels = 3;

    // Windows.UI.Composition needs a dispatcher queue on the UI thread; one is enough for the app.
    private static IntPtr s_dispatcherQueueController;

    private readonly IntPtr _window;
    private readonly Compositor _compositor;
    private readonly DesktopWindowTarget _target;
    private readonly SpriteVisual _blur;
    private readonly CompositionRoundedRectangleGeometry _shape;
    private (int Left, int Top, int Width, int Height, int Radius) _shownRegion;

    /// <summary>Sets the glass window up for the blur. Throws when Windows refuses; the caller shows Solid then.</summary>
    public GlassBackdrop(IntPtr window)
    {
        if (window == IntPtr.Zero)
        {
            throw new ArgumentException("The blur window has no handle yet.", nameof(window));
        }

        _window = window;
        int noRounding = NativeMethods.DwmwcpDoNotRound;
        Marshal.ThrowExceptionForHR(NativeMethods.DwmSetWindowAttribute(window, NativeMethods.DwmwaWindowCornerPreference, ref noRounding, sizeof(int)));
        int useHostBackdrop = 1;
        Marshal.ThrowExceptionForHR(NativeMethods.DwmSetWindowAttribute(window, NativeMethods.DwmwaUseHostBackdropBrush, ref useHostBackdrop, sizeof(int)));

        EnsureDispatcherQueue();
        _compositor = new Compositor();
        _target = CreateTarget(_compositor, window);
        _shape = _compositor.CreateRoundedRectangleGeometry();
        _blur = _compositor.CreateSpriteVisual();
        _blur.Brush = _compositor.CreateHostBackdropBrush();
        _blur.Clip = _compositor.CreateGeometricClip(_shape);
        _target.Root = _blur;
    }

    /// <summary>Places the blur over the island; pixels relative to the window, opacity 0 to 1.</summary>
    public void Update(int left, int top, int width, int height, int radius, float opacity)
    {
        _blur.Opacity = opacity;
        _blur.Offset = new Vector3(left, top, 0);
        Vector2 size = new(width, height);
        _blur.Size = size;
        _shape.Size = size;
        _shape.CornerRadius = new Vector2(radius, radius);

        (int, int, int, int, int) region = (left, top, width, height, radius);
        if (region != _shownRegion)
        {
            _shownRegion = region;
            int margin = RegionMarginPixels;
            int corner = 2 * (radius + margin);
            NativeMethods.SetWindowRgn(_window, NativeMethods.CreateRoundRectRgn(left - margin, top - margin, left + width + margin + 1, top + height + margin + 1, corner, corner), true);
        }
    }

    /// <summary>Puts the blur window directly under the given window (the island), without activating either.</summary>
    public void PlaceBelow(IntPtr window)
    {
        NativeMethods.SetWindowPos(_window, window, 0, 0, 0, 0, NativeMethods.SwpNoMove | NativeMethods.SwpNoSize | NativeMethods.SwpNoActivate);
    }

    /// <summary>A fresh blur brush, so Windows samples what is behind the island again.</summary>
    public void RefreshBackdrop()
    {
        _blur.Brush = _compositor.CreateHostBackdropBrush();
    }

    private static void EnsureDispatcherQueue()
    {
        if (s_dispatcherQueueController != IntPtr.Zero)
        {
            return;
        }

        NativeMethods.DispatcherQueueOptions options = new()
        {
            Size = Marshal.SizeOf<NativeMethods.DispatcherQueueOptions>(),
            ThreadType = NativeMethods.DqTypeThreadCurrent,
            ApartmentType = NativeMethods.DqtatComSta,
        };
        Marshal.ThrowExceptionForHR(NativeMethods.CreateDispatcherQueueController(options, out s_dispatcherQueueController));
    }

    // ICompositorDesktopInterop has no .NET projection, so its one method is called through the
    // vtable: slot 3, right after IUnknown's three. isTopmost false puts the blur behind the window.
    private static unsafe DesktopWindowTarget CreateTarget(Compositor compositor, IntPtr window)
    {
        IntPtr compositorPointer = ((IWinRTObject)compositor).NativeObject.ThisPtr;
        Guid interopId = CompositorDesktopInteropId;
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(compositorPointer, in interopId, out IntPtr interop));
        try
        {
            delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr*, int> createDesktopWindowTarget =
                (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr*, int>)(*(*(void***)interop + 3));
            IntPtr targetPointer;
            Marshal.ThrowExceptionForHR(createDesktopWindowTarget(interop, window, 0, &targetPointer));
            DesktopWindowTarget target = DesktopWindowTarget.FromAbi(targetPointer);
            Marshal.Release(targetPointer);
            return target;
        }
        finally
        {
            Marshal.Release(interop);
        }
    }
}
