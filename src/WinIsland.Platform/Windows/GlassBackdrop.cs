// GlassBackdrop.cs: puts Windows' own blur into a window, cut to the island's shape (PRD FR-37).
// This is the way the v1.4 glass spike proved after 10 rounds (docs/spike-results.md):
//   - The blur is Windows.UI.Composition's host backdrop brush (documented). Windows draws it and
//     keeps drawing it while the window is not active, which the island never is.
//   - It lives in an empty window of its own behind the island, because in a window with WPF
//     content it hides that content (round 7).
//   - It is clipped to a rounded rectangle with smooth edges, 1 pixel larger than the island, and
//     the window's hard region is the island's shape itself, so the hard cut only ever crosses
//     blur and never shows a dark rim (round 10).
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
    private const int BleedPixels = 1;

    // Windows.UI.Composition needs a dispatcher queue on the UI thread; one is enough for the app.
    private static IntPtr s_dispatcherQueueController;

    private readonly IntPtr _window;
    private readonly Compositor _compositor;
    private readonly DesktopWindowTarget _target;
    private readonly SpriteVisual _blur;
    private readonly CompositionRoundedRectangleGeometry _shape;
    private (int Left, int Top, int Width, int Height, int Radius) _shownRegion;

    /// <summary>Sets the window up for the blur. Throws when Windows refuses; the caller shows Solid then.</summary>
    public GlassBackdrop(IntPtr window)
    {
        if (window == IntPtr.Zero)
        {
            throw new ArgumentException("The blur window has no handle yet.", nameof(window));
        }

        _window = window;
        MakeClickThroughAndNonActivating(window);

        NativeMethods.Margins wholeWindow = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        Marshal.ThrowExceptionForHR(NativeMethods.DwmExtendFrameIntoClientArea(window, ref wholeWindow));
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
        _blur.Offset = new Vector3(left - BleedPixels, top, 0);
        Vector2 size = new(width + (2 * BleedPixels), height + BleedPixels);
        _blur.Size = size;
        _shape.Size = size;
        _shape.CornerRadius = new Vector2(radius + BleedPixels, radius + BleedPixels);

        (int, int, int, int, int) region = (left, top, width, height, radius);
        if (region != _shownRegion)
        {
            _shownRegion = region;
            int corner = 2 * radius;
            NativeMethods.SetWindowRgn(_window, NativeMethods.CreateRoundRectRgn(left, top, left + width + 1, top + height + 1, corner, corner), true);
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

    private static void MakeClickThroughAndNonActivating(IntPtr window)
    {
        long style = NativeMethods.GetWindowLongPtr(window, NativeMethods.GwlExStyle).ToInt64();
        style |= NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow | NativeMethods.WsExTopmost | NativeMethods.WsExTransparent;
        Marshal.SetLastPInvokeError(0);
        IntPtr previous = NativeMethods.SetWindowLongPtr(window, NativeMethods.GwlExStyle, new IntPtr(style));
        int error = Marshal.GetLastPInvokeError();
        if (previous == IntPtr.Zero && error != 0)
        {
            throw new Win32Exception(error, "Could not set up the blur window.");
        }
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
