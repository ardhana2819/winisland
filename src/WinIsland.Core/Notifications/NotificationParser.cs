// NotificationParser.cs: turns a Windows toast into what the island shows (PRD FR-13).
// Spike U7 showed the layout: the first text line is the sender or chat name, the rest is the
// message (in WhatsApp groups the message starts with "sender: "). Discord uses the same order.
//   Section 13: a notification with no text shows the app only; one with a single line (for
//   example a WhatsApp photo) shows that line as the sender and no preview.
//   Line breaks inside a text become spaces, so a card never shows more lines than it has room for.
//   v1.2: WhatsApp Web from Chrome has the same layout (spike W2) and counts as WhatsApp (FR-28).

namespace WinIsland.Core.Notifications;

public static class NotificationParser
{
    public static IslandNotification Parse(RawNotification raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        string appName = string.IsNullOrWhiteSpace(raw.AppName) ? "App" : raw.AppName.Trim();
        List<string> lines = (raw.Texts ?? [])
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(OneLine)
            .ToList();

        string sender = lines.Count > 0 ? lines[0] : appName;
        string preview = lines.Count > 1 ? string.Join(" ", lines.Skip(1)) : string.Empty;

        return new IslandNotification(raw.WindowsId, SourceOf(raw), appName, sender, preview, raw.CreatedAt, IsWhatsAppWeb(raw));
    }

    /// <summary>
    /// WhatsApp Web installed as an app from Chrome (PRD FR-28, spike W1): the app name says
    /// "WhatsApp Web", but the app id is Chrome's, not the WhatsApp desktop app's.
    /// </summary>
    public static bool IsWhatsAppWeb(RawNotification raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        string appId = raw.AppUserModelId ?? string.Empty;
        string appName = raw.AppName ?? string.Empty;
        return !appId.Contains("WhatsApp", StringComparison.OrdinalIgnoreCase)
            && appName.Contains("WhatsApp", StringComparison.OrdinalIgnoreCase);
    }

    // A long WhatsApp group message can carry its own line breaks. The island shows one line per
    // text (Diva's bug, 1 Nov 2026: the extra lines spilled out of the card), so every run of
    // spaces, tabs and line breaks becomes one space.
    private static string OneLine(string text)
    {
        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static NotificationSource SourceOf(RawNotification raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        // Spike U7: WhatsApp is "5319275A.WhatsAppDesktop_...!App", Discord "com.squirrel.Discord.Discord".
        string appId = raw.AppUserModelId ?? string.Empty;
        if (appId.Contains("WhatsApp", StringComparison.OrdinalIgnoreCase) || IsWhatsAppWeb(raw))
        {
            return NotificationSource.WhatsApp;
        }

        if (appId.Contains("Discord", StringComparison.OrdinalIgnoreCase))
        {
            return NotificationSource.Discord;
        }

        return NotificationSource.Other;
    }
}
