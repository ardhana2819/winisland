// SettingsFile.cs: reads and writes WinIsland's settings file (PRD FR-19, "saved locally"):
// %USERPROFILE%\.winisland\settings.json, for this Windows user only. It holds settings only,
// never message text (FR-18, AC-9).
// Saving writes a temporary file first and then swaps it in, so a crash or power cut in the
// middle of saving can never leave a half-written settings file.

using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Settings;

public static class SettingsFile
{
    public static string FolderPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".winisland");

    public static string FilePath { get; } = Path.Combine(FolderPath, "settings.json");

    /// <summary>The saved text, or null when there is no file yet or it cannot be read.</summary>
    public static string? Load()
    {
        try
        {
            return File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            PlatformLog.Problem("Reading the settings file failed", error);
            return null;
        }
    }

    /// <summary>Saves the text. Returns false when Windows refused; the old file then stays.</summary>
    public static bool Save(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string temporaryPath = FilePath + ".new";
        try
        {
            Directory.CreateDirectory(FolderPath);
            File.WriteAllText(temporaryPath, text);
            File.Move(temporaryPath, FilePath, overwrite: true);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            PlatformLog.Problem("Saving the settings file failed", error);
            return false;
        }
    }
}
