// AppSettings.cs: everything the Settings window can change (PRD section 4, FR-19, FR-20):
// hotkeys, which apps show notifications, hide message text, banner time, launch with Windows,
// (v1.3, FR-32) the island size, and (v1.4, FR-37) the look.
// Defaults follow the PRD and the design board. The rules each setting must follow are here too,
// so the Settings window and the saved file are checked the same way.

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Settings;

public sealed record AppSettings(HotkeySet Hotkeys, NotificationSettings Notifications, bool LaunchAtStartup, IslandSize Size = IslandSize.Medium, IslandLook Look = IslandLook.Solid)
{
    /// <summary>PRD section 12 asks for a sane range; Diva chose 3 to 15 seconds on 31 Oct 2026.</summary>
    public static readonly TimeSpan MinBannerDuration = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan MaxBannerDuration = TimeSpan.FromSeconds(15);

    /// <summary>Design board: launch with Windows is on by default (PRD section 4).</summary>
    public static AppSettings Defaults { get; } = new(HotkeySet.Defaults, NotificationSettings.Defaults, LaunchAtStartup: true);

    /// <summary>Returns a plain-English problem for each broken rule. Empty means the settings are valid.</summary>
    public IReadOnlyList<string> FindProblems()
    {
        List<string> problems = [.. Hotkeys.FindProblems()];

        TimeSpan duration = Notifications.BannerDuration;
        if (duration < MinBannerDuration || duration > MaxBannerDuration)
        {
            problems.Add($"Notification duration must be between {MinBannerDuration.TotalSeconds:0} and {MaxBannerDuration.TotalSeconds:0} seconds.");
        }

        if (!Enum.IsDefined(Size))
        {
            problems.Add("Island size must be Small, Medium, Large or Extra large.");
        }

        if (!Enum.IsDefined(Look))
        {
            problems.Add("Look must be Solid or Glass.");
        }

        return problems;
    }
}
