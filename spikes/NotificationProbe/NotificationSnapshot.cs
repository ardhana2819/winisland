// NotificationSnapshot.cs: the parts of one Windows notification the spike cares about.
// Plain data, held in memory only.

namespace NotificationProbe;

internal sealed record NotificationSnapshot(
    uint Id,
    string AppName,
    string AppUserModelId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Texts);
