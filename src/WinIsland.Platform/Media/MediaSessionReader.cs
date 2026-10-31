// MediaSessionReader.cs: turns what the Windows media session reports into Core's NowPlaying.
// Kept separate from SpotifyMediaSession so the translation rules sit in one small place.

using Windows.Media.Control;

using WinIsland.Core.Music;

namespace WinIsland.Platform.Media;

internal static class MediaSessionReader
{
    public static NowPlaying Read(
        GlobalSystemMediaTransportControlsSession session,
        GlobalSystemMediaTransportControlsSessionMediaProperties properties,
        DateTimeOffset now)
    {
        GlobalSystemMediaTransportControlsSessionPlaybackInfo playback = session.GetPlaybackInfo();
        GlobalSystemMediaTransportControlsSessionTimelineProperties timeline = session.GetTimelineProperties();

        TimeSpan duration = timeline.EndTime - timeline.StartTime;
        TimeSpan position = timeline.Position - timeline.StartTime;

        return new NowPlaying(
            properties.Title,
            properties.Artist,
            ToStatus(playback.PlaybackStatus),
            duration < TimeSpan.Zero ? TimeSpan.Zero : duration,
            position,
            UsableUpdateTime(timeline.LastUpdatedTime, now),
            playback.Controls.IsPlaybackPositionEnabled);
    }

    private static PlaybackStatus ToStatus(GlobalSystemMediaTransportControlsSessionPlaybackStatus status)
    {
        return status switch
        {
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => PlaybackStatus.Playing,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => PlaybackStatus.Paused,
            _ => PlaybackStatus.Stopped,
        };
    }

    // Some players never fill in the update time (it comes back as year 1601) or report a time
    // slightly in the future. In both cases "now" is the best guess.
    private static DateTimeOffset UsableUpdateTime(DateTimeOffset reported, DateTimeOffset now)
    {
        bool isUnset = reported.Year < 2000;
        bool isInFuture = reported > now;
        return isUnset || isInFuture ? now : reported;
    }
}
