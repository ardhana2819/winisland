// RawNotification.cs: a Windows toast notification as Platform reads it, before the island
// interprets it. Texts are the toast's text lines in order (spike U7: WhatsApp sends the chat
// name first and the message second). Held in memory only (PRD FR-18).

namespace WinIsland.Core.Notifications;

public sealed record RawNotification(
    uint WindowsId,
    string AppUserModelId,
    string AppName,
    IReadOnlyList<string> Texts,
    DateTimeOffset CreatedAt);
