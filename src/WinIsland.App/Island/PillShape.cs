// PillShape.cs: the size and corner radius of the island for each view, from DESIGN_SPEC section 2.
// All values are in DIPs at the base scale; the window scales everything as a whole (FR-1).

using WinIsland.Core.Island;

namespace WinIsland.App.Island;

internal readonly record struct PillShape(double Width, double Height, double CornerRadius)
{
    public static PillShape For(IslandView view)
    {
        return view switch
        {
            IslandView.Compact => new PillShape(120, 34, 17),
            IslandView.Music => new PillShape(280, 36, 18),
            IslandView.Expanded => new PillShape(420, 168, 28),
            IslandView.HotkeyConflict => new PillShape(400, 52, 26),
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "No island shape is defined for this view."),
        };
    }
}
