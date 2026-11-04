// BlurWindow.cs: the empty window right behind the island that carries the Glass blur (PRD FR-37).
// It has no content of its own: Windows' blur (GlassBackdrop in Platform) would hide WPF content
// in the same window, which is why the island cannot carry it itself (glass spike round 7).
// v1.5 polish: it is a plain Windows window (GlassWindow in Platform), not a WPF one, because
// only such a window can be see-through around the blur, which gives the island smooth edges.
// It never takes focus, clicks pass through it, and it stays on top like the island.
// If Windows refuses the blur, Backdrop stays null and Problem says why; the island then shows Solid.

using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

internal sealed class BlurWindow : IDisposable
{
    private readonly GlassWindow? _window;
    private readonly TopmostGuard? _topmostGuard;

    public BlurWindow()
    {
        GlassWindow? window = null;
        try
        {
            window = new GlassWindow();
            Backdrop = new GlassBackdrop(window.Handle);
            _topmostGuard = new TopmostGuard(window.Handle);
            _window = window;
        }
        catch (Exception error)
        {
            // Reported to the caller, which logs it and falls back to Solid (PRD FR-37).
            Problem = error;
            window?.Dispose();
        }
    }

    public GlassBackdrop? Backdrop { get; }

    /// <summary>Why the blur could not start, or null.</summary>
    public Exception? Problem { get; }

    public IntPtr Handle => _window?.Handle ?? IntPtr.Zero;

    public void Show() => _window?.Show();

    /// <summary>Gives the blur window the island window's place and size.</summary>
    public void PlaceOver(IntPtr island) => _window?.PlaceOver(island);

    public void Dispose()
    {
        _topmostGuard?.Dispose();
        _window?.Dispose();
    }
}
