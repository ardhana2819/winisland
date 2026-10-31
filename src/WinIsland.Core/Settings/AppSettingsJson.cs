// AppSettingsJson.cs: turns the settings into the text saved on disk (PRD FR-19) and back.
// The file holds settings only, never message text (FR-18, AC-9). Hotkeys are saved as readable
// text ("Ctrl+Alt+I") and the banner time in whole seconds, so the file is easy to read and fix by
// hand. Anything missing, unreadable or breaking a rule falls back to its default, so a damaged
// file never stops WinIsland from starting.

using System.Text.Encodings.Web;
using System.Text.Json;

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Notifications;

namespace WinIsland.Core.Settings;

public static class AppSettingsJson
{
    // The default encoder writes "+" as "+" to protect web pages. This file never goes into a
    // web page, and "Ctrl+Alt+I" should stay readable when someone opens the file.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Write(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        SettingsFile file = new()
        {
            ExpandHotkey = settings.Hotkeys.Expand.ToString(),
            HideShowHotkey = settings.Hotkeys.HideShow.ToString(),
            HideNotificationsHotkey = settings.Hotkeys.HideBanner.ToString(),
            NotificationSeconds = (int)settings.Notifications.BannerDuration.TotalSeconds,
            ShowWhatsApp = settings.Notifications.ShowWhatsApp,
            ShowDiscord = settings.Notifications.ShowDiscord,
            ShowOtherApps = settings.Notifications.ShowOtherApps,
            HideMessageText = settings.Notifications.HideMessageText,
            LaunchAtStartup = settings.LaunchAtStartup,
        };

        return JsonSerializer.Serialize(file, Options);
    }

    /// <summary>Reads saved settings. Missing or broken values use the defaults.</summary>
    public static AppSettings Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return AppSettings.Defaults;
        }

        SettingsFile? file;
        try
        {
            file = JsonSerializer.Deserialize<SettingsFile>(json, Options);
        }
        catch (JsonException)
        {
            return AppSettings.Defaults;
        }

        if (file is null)
        {
            return AppSettings.Defaults;
        }

        AppSettings defaults = AppSettings.Defaults;
        HotkeySet hotkeys = new(
            Hotkey.TryParse(file.ExpandHotkey) ?? defaults.Hotkeys.Expand,
            Hotkey.TryParse(file.HideShowHotkey) ?? defaults.Hotkeys.HideShow,
            Hotkey.TryParse(file.HideNotificationsHotkey) ?? defaults.Hotkeys.HideBanner);
        if (hotkeys.FindProblems().Count > 0)
        {
            hotkeys = defaults.Hotkeys;
        }

        TimeSpan duration = file.NotificationSeconds is int seconds ? TimeSpan.FromSeconds(seconds) : defaults.Notifications.BannerDuration;
        if (duration < AppSettings.MinBannerDuration || duration > AppSettings.MaxBannerDuration)
        {
            duration = defaults.Notifications.BannerDuration;
        }

        NotificationSettings notifications = new(
            file.ShowWhatsApp ?? defaults.Notifications.ShowWhatsApp,
            file.ShowDiscord ?? defaults.Notifications.ShowDiscord,
            file.ShowOtherApps ?? defaults.Notifications.ShowOtherApps,
            file.HideMessageText ?? defaults.Notifications.HideMessageText,
            duration);

        return new AppSettings(hotkeys, notifications, file.LaunchAtStartup ?? defaults.LaunchAtStartup);
    }

    // The shape of the file. Every value is optional, so older or hand-edited files still load.
    private sealed class SettingsFile
    {
        public string? ExpandHotkey { get; set; }

        public string? HideShowHotkey { get; set; }

        public string? HideNotificationsHotkey { get; set; }

        public int? NotificationSeconds { get; set; }

        public bool? ShowWhatsApp { get; set; }

        public bool? ShowDiscord { get; set; }

        public bool? ShowOtherApps { get; set; }

        public bool? HideMessageText { get; set; }

        public bool? LaunchAtStartup { get; set; }
    }
}
