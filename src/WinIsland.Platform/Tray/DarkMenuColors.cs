// DarkMenuColors.cs: the tray menu colors from the design board ("Settings and tray"):
// menu background #1C1C1E, border #2C2C2E, hover #2C2C2E, text #F5F5F7.
// WinForms menus are light by default; this color table makes them match the dark design.

namespace WinIsland.Platform.Tray;

internal sealed class DarkMenuColors : ProfessionalColorTable
{
    public static readonly Color Background = Color.FromArgb(0x1C, 0x1C, 0x1E);
    public static readonly Color Hover = Color.FromArgb(0x2C, 0x2C, 0x2E);
    public static readonly Color Text = Color.FromArgb(0xF5, 0xF5, 0xF7);

    public override Color ToolStripDropDownBackground => Background;

    public override Color ImageMarginGradientBegin => Background;

    public override Color ImageMarginGradientMiddle => Background;

    public override Color ImageMarginGradientEnd => Background;

    public override Color MenuBorder => Hover;

    public override Color MenuItemBorder => Hover;

    public override Color MenuItemSelected => Hover;

    public override Color MenuItemSelectedGradientBegin => Hover;

    public override Color MenuItemSelectedGradientEnd => Hover;

    public override Color SeparatorDark => Hover;

    public override Color SeparatorLight => Hover;
}
