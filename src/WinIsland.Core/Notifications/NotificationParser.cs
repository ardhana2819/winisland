// NotificationParser.cs: turns a Windows toast into what the island shows (PRD FR-13).
// Spike U7 showed the layout: the first text line is the sender or chat name, the rest is the
// message (in WhatsApp groups the message starts with "sender: "). Discord uses the same order.
//   Section 13: a notification with no text shows the app only; one with a single line (for
//   example a WhatsApp photo) shows that line as the sender and no preview.

namespace WinIsland.Core.Notifications;

public static class NotificationParser
{
    public static IslandNotification Parse(RawNotification raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        string appName = string.IsNullOrWhiteSpace(raw.AppName) ? "App" : raw.AppName.Trim();
        List<string> lines = (raw.Texts ?? [])
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim())
            .ToList();

        string sender = lines.Count > 0 ? lines[0] : appName;
        string preview = lines.Count > 1 ? string.Join(" ", lines.Skip(1)) : string.Empty;

        return new IslandNotification(raw.WindowsId, SourceOf(raw), appName, sender, preview, raw.CreatedAt);
    }

    public static NotificationSource SourceOf(RawNotification raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        // Spike U7: WhatsApp is "5319275A.WhatsAppDesktop_...!App", Discord "com.squirrel.Discord.Discord".
        string appId = raw.AppUserModelId ?? string.Empty;
        if (appId.Contains("WhatsApp", StringComparison.OrdinalIgnoreCase))
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
