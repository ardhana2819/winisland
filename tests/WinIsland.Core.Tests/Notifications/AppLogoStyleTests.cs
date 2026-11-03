// AppLogoStyleTests.cs: checks how other apps' logos are drawn (PRD FR-42): a see-through logo is a
// white shape on its main colour, a full tile fills the circle, a colourless logo gets grey, and
// the circle stays dark enough for a white logo.

using WinIsland.Core.Music;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class AppLogoStyleTests
{
    private static readonly RgbColor ClaudeOrange = new(0xD9, 0x77, 0x57);

    [Fact]
    public void An_orange_mark_on_a_see_through_background_is_a_white_shape_on_orange()
    {
        byte[] logo = Logo(ClaudeOrange, coloredShare: 0.4);

        AppLogoStyle style = AppLogoStyle.FromBgra32(logo);

        Assert.True(style.IsShape);
        (double hue, _, _) = ColorMath.ToHsl(style.Circle);
        Assert.InRange(hue, 10, 20);
        Assert.True(ColorMath.Contrast(style.Circle, RgbColor.White) >= 2.5);
    }

    [Fact]
    public void A_logo_with_no_see_through_parts_is_a_full_tile()
    {
        byte[] logo = Logo(ClaudeOrange, coloredShare: 1);

        Assert.False(AppLogoStyle.FromBgra32(logo).IsShape);
    }

    [Fact]
    public void A_grey_logo_gets_the_quiet_grey_circle()
    {
        byte[] logo = Logo(new RgbColor(0x80, 0x80, 0x80), coloredShare: 0.5);

        Assert.Equal(AppLogoStyle.NeutralCircle, AppLogoStyle.FromBgra32(logo).Circle);
    }

    [Fact]
    public void A_very_light_logo_colour_is_darkened_so_white_stays_readable()
    {
        byte[] logo = Logo(new RgbColor(0xFF, 0xD6, 0xE0), coloredShare: 0.5);

        (_, _, double lightness) = ColorMath.ToHsl(AppLogoStyle.FromBgra32(logo).Circle);
        Assert.True(lightness <= 0.6);
    }

    [Fact]
    public void An_empty_picture_gets_the_grey_circle()
    {
        Assert.Equal(AppLogoStyle.NeutralCircle, AppLogoStyle.FromBgra32([]).Circle);
    }

    // A 10 x 10 logo: the first share of pixels in the colour, the rest fully see-through.
    private static byte[] Logo(RgbColor color, double coloredShare)
    {
        const int pixels = 100;
        byte[] bgra = new byte[pixels * 4];
        for (int index = 0; index < pixels; index++)
        {
            if (index < pixels * coloredShare)
            {
                bgra[(index * 4) + 0] = color.B;
                bgra[(index * 4) + 1] = color.G;
                bgra[(index * 4) + 2] = color.R;
                bgra[(index * 4) + 3] = 255;
            }
        }

        return bgra;
    }
}
