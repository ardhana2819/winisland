// PauseHold.cs: keeps a paused app on the island for 3 seconds before another app may take its
// place (PRD FR-41, Diva 3 Nov 2026). Without it, pausing Spotify for a moment while a YouTube
// video plays made the island jump to YouTube at once. If the app plays again within 3 seconds,
// nothing changes.

namespace WinIsland.Core.Music;

public sealed class PauseHold
{
    /// <summary>How long a paused app keeps the island (Diva, 3 Nov 2026).</summary>
    public static readonly TimeSpan Delay = TimeSpan.FromSeconds(3);

    private string? _appId;
    private bool _wasPlaying;
    private DateTimeOffset? _stoppedAt;

    /// <summary>
    /// Tells the hold which app the island shows and whether it plays now. Returns how much longer
    /// that app keeps the island after a pause, or zero when it is not held.
    /// </summary>
    public TimeSpan Update(string? shownAppId, bool isShownPlaying, DateTimeOffset now)
    {
        if (!string.Equals(shownAppId, _appId, StringComparison.Ordinal))
        {
            _appId = shownAppId;
            _wasPlaying = isShownPlaying;
            _stoppedAt = null;
            return TimeSpan.Zero;
        }

        if (isShownPlaying)
        {
            _wasPlaying = true;
            _stoppedAt = null;
            return TimeSpan.Zero;
        }

        if (_wasPlaying)
        {
            _wasPlaying = false;
            _stoppedAt = now;
        }

        if (_stoppedAt is not DateTimeOffset stoppedAt)
        {
            return TimeSpan.Zero;
        }

        TimeSpan left = stoppedAt + Delay - now;
        return left > TimeSpan.Zero ? left : TimeSpan.Zero;
    }
}
