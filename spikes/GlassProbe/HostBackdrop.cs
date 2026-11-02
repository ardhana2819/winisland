// HostBackdrop.cs: the documented way to draw Windows' blur inside our own window (spike round 6).
// Windows.UI.Composition puts one visual behind this window's WPF content. Its brush is the host
// backdrop brush, which is Windows' own blur of whatever is behind the window. Its clip is a
// rounded rectangle with smoothed edges, so the blur is exactly pill-shaped and never a box.
// Windows draws the blur; nothing here captures or reads the screen.

using System.Numerics;
using System.Runtime.InteropServices;
using Windows.UI.Composition;
using Windows.UI.Composition.Desktop;
using WinRT;

namespace GlassProbe;

internal sealed class HostBackdrop
{
    // ICompositorDesktopInterop, from windows.ui.composition.interop.h.
    private static readonly Guid CompositorDesktopInteropId = new("29E691FA-4567-4DCA-B319-D0F207EB6807");

    // Kept so the dispatcher queue lives as long as the window; Composition needs one on this thread.
    private readonly IntPtr _dispatcherQueueController;
    private readonly Compositor _compositor;
    private readonly DesktopWindowTarget _target;
    private readonly SpriteVisual _blur;
    private readonly CompositionRoundedRectangleGeometry _shape;

    public HostBackdrop(IntPtr window)
    {
        NativeMethods.DispatcherQueueOptions options = new()
        {
            Size = Marshal.SizeOf<NativeMethods.DispatcherQueueOptions>(),
            ThreadType = NativeMethods.DqTypeThreadCurrent,
            ApartmentType = NativeMethods.DqtatComSta,
        };
        Marshal.ThrowExceptionForHR(NativeMethods.CreateDispatcherQueueController(options, out _dispatcherQueueController));

        int useHostBackdrop = 1;
        Marshal.ThrowExceptionForHR(NativeMethods.DwmSetWindowAttribute(window, NativeMethods.DwmwaUseHostBackdropBrush, ref useHostBackdrop, sizeof(int)));

        _compositor = new Compositor();
        _target = CreateTarget(_compositor, window);

        _shape = _compositor.CreateRoundedRectangleGeometry();
        _blur = _compositor.CreateSpriteVisual();
        _blur.Brush = _compositor.CreateHostBackdropBrush();
        _blur.Clip = _compositor.CreateGeometricClip(_shape);
        _target.Root = _blur;
    }

    /// <summary>Places the blur; all values in physical pixels, relative to the window.</summary>
    public void Update(float left, float width, float height, float radius)
    {
        _blur.Offset = new Vector3(left, 0, 0);
        _blur.Size = new Vector2(width, height);
        _shape.Size = new Vector2(width, height);
        _shape.CornerRadius = new Vector2(radius, radius);
    }

    // ICompositorDesktopInterop has no projection in .NET, so its one method is called through the
    // vtable: slot 3, right after IUnknown's three. isTopmost false puts the blur behind WPF's content.
    private static unsafe DesktopWindowTarget CreateTarget(Compositor compositor, IntPtr window)
    {
        IntPtr compositorPointer = ((IWinRTObject)compositor).NativeObject.ThisPtr;
        Guid interopId = CompositorDesktopInteropId;
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(compositorPointer, in interopId, out IntPtr interop));
        try
        {
            delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr*, int> createDesktopWindowTarget = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr*, int>)(*(*(void***)interop + 3));
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
