// GlassWindow.cs: the empty window behind the island that carries the Glass blur (PRD FR-37).
// It is made as a window that shows only what Windows' composition draws in it (no redirection
// bitmap, a documented window style), so everything outside the blur is truly see-through. The
// earlier WPF window painted black outside the blur, so it had to be cut to the exact pill with a
// window region, and a region has no edge smoothing: Diva saw rough edges after a few days of Glass
// (4 Nov 2026). Now the blur's own smooth rounded clip makes the edge.
// It never takes focus, clicks pass through it, and it stays on top like the island.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WinIsland.Platform.Windows;

public sealed class GlassWindow : IDisposable
{
    private const string ClassName = "WinIslandGlass";
    private const uint MouseActivateMessage = 0x0021;

    private static bool s_isClassRegistered;

    private NativeMethods.Rect _placed;

    /// <summary>Creates the window, not shown yet. Throws when Windows refuses.</summary>
    public GlassWindow()
    {
        RegisterClassOnce();
        long style = NativeMethods.WsExNoRedirectionBitmap | NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow
            | NativeMethods.WsExTopmost | NativeMethods.WsExTransparent;
        Handle = NativeMethods.CreateWindowEx((uint)style, ClassName, "WinIsland glass", NativeMethods.WsPopup, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, NativeMethods.GetModuleHandle(IntPtr.Zero), IntPtr.Zero);
        if (Handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not create the blur window.");
        }
    }

    public IntPtr Handle { get; private set; }

    public void Show() => NativeMethods.ShowWindow(Handle, NativeMethods.SwShowNoActivate);

    /// <summary>Gives the window the same place and size as the other window (the island), in pixels.</summary>
    public void PlaceOver(IntPtr window)
    {
        if (Handle == IntPtr.Zero || !NativeMethods.GetWindowRect(window, out NativeMethods.Rect rect) || rect.Equals(_placed))
        {
            return;
        }

        _placed = rect;
        NativeMethods.SetWindowPos(Handle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, NativeMethods.SwpNoZOrder | NativeMethods.SwpNoActivate);
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            NativeMethods.DestroyWindow(Handle);
            Handle = IntPtr.Zero;
        }
    }

    private static unsafe void RegisterClassOnce()
    {
        if (s_isClassRegistered)
        {
            return;
        }

        fixed (char* name = ClassName)
        {
            NativeMethods.WindowClass windowClass = new()
            {
                Size = sizeof(NativeMethods.WindowClass),
                WindowProcedure = &WindowProcedure,
                Instance = NativeMethods.GetModuleHandle(IntPtr.Zero),
                ClassName = (IntPtr)name,
            };
            if (NativeMethods.RegisterClassEx(in windowClass) == 0)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not register the blur window.");
            }
        }

        s_isClassRegistered = true;
    }

    // A click never activates the window; everything else is Windows' default.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static IntPtr WindowProcedure(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
    {
        return message == MouseActivateMessage ? (IntPtr)NativeMethods.MaNoActivate : NativeMethods.DefWindowProc(window, message, wParam, lParam);
    }
}
