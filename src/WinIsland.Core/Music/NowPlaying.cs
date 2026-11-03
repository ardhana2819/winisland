// NowPlaying.cs: what the music player reports right now (PRD FR-7).
// Position is the position at the moment PositionUpdatedAt; the island estimates the current
// position from it between updates (FR-9, see PlaybackPosition.cs).
// Artwork is not part of this record: it is an image the app loads separately.
// AppId (v1.5) names the app the music comes from, as Windows' media session names it, so the
// volume slider can find that app's sound (FR-39, AudioAppMatch.cs).

namespace WinIsland.Core.Music;

public sealed record NowPlaying
{
    public NowPlaying(
        string title,
        string artist,
        PlaybackStatus status,
        TimeSpan duration,
        TimeSpan position,
        DateTimeOffset positionUpdatedAt,
        bool canSeek,
        string appId = "")
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "A track cannot have a negative length.");
        }

        Title = title ?? string.Empty;
        Artist = artist ?? string.Empty;
        Status = status;
        Duration = duration;
        Position = position < TimeSpan.Zero ? TimeSpan.Zero : position;
        PositionUpdatedAt = positionUpdatedAt;
        CanSeek = canSeek;
        AppId = appId ?? string.Empty;
    }

    public string Title { get; }

    public string Artist { get; }

    public PlaybackStatus Status { get; }

    public TimeSpan Duration { get; }

    public TimeSpan Position { get; }

    public DateTimeOffset PositionUpdatedAt { get; }

    /// <summary>True when the player accepts seeking (FR-11). Spotify does (spike U6).</summary>
    public bool CanSeek { get; }

    public bool IsPlaying => Status == PlaybackStatus.Playing;

    /// <summary>The music app's id in Windows' media session, for example "Spotify.exe" (FR-39, FR-41).</summary>
    public string AppId { get; }
}
