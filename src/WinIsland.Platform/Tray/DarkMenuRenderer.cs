// DarkMenuRenderer.cs: draws the tray menu with the dark design colors (see DarkMenuColors.cs).
// The color table covers backgrounds and hover; this renderer also makes the text light.

namespace WinIsland.Platform.Tray;

internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer()
        : base(new DarkMenuColors())
    {
        RoundedEdges = false;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = DarkMenuColors.Text;
        base.OnRenderItemText(e);
    }
}
