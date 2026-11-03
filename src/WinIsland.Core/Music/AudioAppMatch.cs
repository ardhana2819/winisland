// AudioAppMatch.cs: tells whether a sound in the Windows Volume Mixer belongs to the app the music
// comes from (PRD FR-39). The media session names its app one way, the mixer another:
//   - a Store app's media id is "<package family>!<app>" (Apple Music, the Store Spotify); its
//     sound belongs to a process of that package family;
//   - a desktop app's media id is its program name, sometimes with ".exe" or with more after a dot
//     ("Spotify.exe", "Chrome", "Chrome._crx_..." for a web app installed from Chrome, "MSEdge");
//     its sound belongs to the process with that name.
// A browser plays every tab through one audio process, so its volume is the whole browser's.
// Apps whose media id matches neither (for example Firefox, which uses a code) get no slider.

namespace WinIsland.Core.Music;

public static class AudioAppMatch
{
    // Media ids that differ from the program's process name.
    private static readonly Dictionary<string, string> ProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MSEdge"] = "msedge",
    };

    /// <summary>True when the process making a sound is the music app.</summary>
    public static bool IsMusicApp(string? mediaAppId, string? processName, string? packageFamilyName)
    {
        if (string.IsNullOrWhiteSpace(mediaAppId))
        {
            return false;
        }

        int bang = mediaAppId.IndexOf('!', StringComparison.Ordinal);
        if (bang > 0)
        {
            return string.Equals(mediaAppId[..bang], packageFamilyName, StringComparison.OrdinalIgnoreCase);
        }

        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        string program = mediaAppId.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? mediaAppId[..^4] : mediaAppId;
        int dot = program.IndexOf('.', StringComparison.Ordinal);
        if (dot > 0)
        {
            program = program[..dot];
        }

        string expected = ProcessNames.GetValueOrDefault(program, program);
        return string.Equals(expected, processName, StringComparison.OrdinalIgnoreCase);
    }
}
