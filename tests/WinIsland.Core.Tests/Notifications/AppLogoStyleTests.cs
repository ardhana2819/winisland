// AppLogoStyleTests.cs: checks how other apps' logos are drawn (PRD FR-42): a thin mark is a white
// shape on its main colour, a plate fills the circle as it is, empty margins are cut off, a
// colourless logo gets grey, and the circle stays dark enough for a white logo.

using WinIsland.Core.Music;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class AppLogoStyleTests
{
    private const int Size = 20;
    private static readonly RgbColor ClaudeOrange = new(0xD9, 0x77, 0x57);

    [Fact]
    public void A_thin_orange_mark_is_a_white_shape_on_orange()
    {
        AppLogoStyle style = AppLogoStyle.FromBgra32(Draw(ClaudeOrange, (x, y) => x == y || x + y == Size - 1 || x == Size / 2), Size);

        Assert.True(style.IsShape);
        (double hue, _, _) = ColorMath.ToHsl(style.Circle);
        Assert.InRange(hue, 10, 20);
        Assert.True(ColorMath.Contrast(style.Circle, RgbColor.White) >= 2.5);
    }

    [Fact]
    public void A_plate_with_empty_margins_fills_the_circle_cut_to_its_visible_part()
    {
        // A filled 10 x 10 square in the middle of a 20 x 20 picture, like Snipping Tool's logo.
        AppLogoStyle style = AppLogoStyle.FromBgra32(Draw(ClaudeOrange, (x, y) => x is >= 5 and < 15 && y is >= 5 and < 15), Size);

        Assert.False(style.IsShape);
        Assert.Equal(new LogoBounds(0.25, 0.25, 0.5, 0.5), style.Visible);
    }

    [Fact]
    public void A_logo_with_no_see_through_parts_is_a_full_plate()
    {
        AppLogoStyle style = AppLogoStyle.FromBgra32(Draw(ClaudeOrange, (_, _) => true), Size);

        Assert.False(style.IsShape);
        Assert.Equal(LogoBounds.Whole, style.Visible);
    }

    [Fact]
    public void A_grey_logo_gets_the_quiet_grey_circle()
    {
        AppLogoStyle style = AppLogoStyle.FromBgra32(Draw(new RgbColor(0x80, 0x80, 0x80), (x, y) => x == y), Size);

        Assert.Equal(AppLogoStyle.NeutralCircle, style.Circle);
    }

    [Fact]
    public void A_very_light_logo_colour_is_darkened_so_white_stays_readable()
    {
        AppLogoStyle style = AppLogoStyle.FromBgra32(Draw(new RgbColor(0xFF, 0xD6, 0xE0), (x, y) => x == y), Size);

        (_, _, double lightness) = ColorMath.ToHsl(style.Circle);
        Assert.True(lightness <= 0.6);
    }

    [Fact]
    public void An_empty_picture_gets_the_grey_circle()
    {
        Assert.Equal(AppLogoStyle.NeutralCircle, AppLogoStyle.FromBgra32([], Size).Circle);
    }

    // A Size x Size logo: the colour where isInk says so, fully see-through elsewhere.
    private static byte[] Draw(RgbColor color, Func<int, int, bool> isInk)
    {
        byte[] bgra = new byte[Size * Size * 4];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                if (!isInk(x, y))
                {
                    continue;
                }

                int index = ((y * Size) + x) * 4;
                bgra[index] = color.B;
                bgra[index + 1] = color.G;
                bgra[index + 2] = color.R;
                bgra[index + 3] = 255;
            }
        }

        return bgra;
    }
}
