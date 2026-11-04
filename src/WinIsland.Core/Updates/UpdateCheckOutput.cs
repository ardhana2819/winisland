// UpdateCheckOutput.cs: reads the result line of the update script's quiet check (PRD FR-46), so
// the Settings window can show "Update available" under its Update button (Diva, 4 Nov 2026).
// The script ends with one of:
//   WINISLAND-UPDATE: available 1.7.0-beta.1
//   WINISLAND-UPDATE: latest
//   WINISLAND-UPDATE: unknown
// Only "available" with a plain version shows the note; anything else shows nothing, so an
// offline PC or a missing GitHub CLI never shows an error in Settings.

namespace WinIsland.Core.Updates;

public static class UpdateCheckOutput
{
    public const string Marker = "WINISLAND-UPDATE:";
    private const string Available = "available ";

    /// <summary>The newer version the check found, or null when there is none or the result is unclear.</summary>
    public static string? AvailableVersion(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        string? result = output
            .Split('\n')
            .Select(line => line.Trim())
            .LastOrDefault(line => line.StartsWith(Marker, StringComparison.Ordinal));
        if (result is null)
        {
            return null;
        }

        string answer = result[Marker.Length..].Trim();
        if (!answer.StartsWith(Available, StringComparison.Ordinal))
        {
            return null;
        }

        string version = answer[Available.Length..].Trim();
        return IsPlainVersion(version) ? version : null;
    }

    // Digits, letters, dots and dashes only, starting with a digit ("1.7.0-beta.1").
    private static bool IsPlainVersion(string version) =>
        version.Length is > 0 and <= 40
        && char.IsAsciiDigit(version[0])
        && version.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-');
}
