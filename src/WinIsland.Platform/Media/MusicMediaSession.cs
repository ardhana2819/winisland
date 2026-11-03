// MusicMediaSession.cs: connects WinIsland to the music app through the Windows media session API
// (Windows.Media.Control), the only music source the PRD allows. No music service's web API, no
// network. v1.5 (FR-41): any app that tells Windows what it plays, not only Spotify; which one
// shows follows MusicSource.Choose, checked again whenever a session starts or stops playing.
//   FR-7  reads title, artist, artwork, play state, position and length.
//   FR-8  sends play/pause, next and previous.
//   FR-10 follows the app opening, closing and changing songs without errors.
//   FR-11 sends seek, when the app says it supports it (spike U6: Spotify does).
// Windows raises its events on background threads; the app moves the results to the UI thread.

using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Media.Control;
using Windows.Storage.Streams;

using WinIsland.Core.Music;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Media;

public sealed class MusicMediaSession : IDisposable
{
    // Album art is small; anything bigger than this is not worth holding in memory.
    private const ulong MaxArtworkBytes = 4 * 1024 * 1024;

    // FR-27: for the first song Spotify can report the title before the picture is ready (no
    // picture, or an empty one), and then send no further event while that song plays. So a missing
    // picture is read again a few times, a second apart, instead of being given up on.
    private const int MaxArtworkRetries = 5;
    private static readonly TimeSpan ArtworkRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan[] LatePictureChecks = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)];

    private readonly GlobalSystemMediaTransportControlsSessionManager _manager;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private readonly SemaphoreSlim _chooseGate = new(1, 1);
    private readonly List<GlobalSystemMediaTransportControlsSession> _watched = [];
    private GlobalSystemMediaTransportControlsSession? _session;
    private string? _artworkTrackKey;
    private byte[]? _shownArtwork;
    private bool _isArtworkShown;
    private string? _missingArtworkTrackKey;
    private int _artworkRetries;
    private bool _isDisposed;

    private MusicMediaSession(GlobalSystemMediaTransportControlsSessionManager manager)
    {
        _manager = manager;
        _manager.SessionsChanged += OnSessionsChanged;
        _manager.CurrentSessionChanged += OnCurrentSessionChanged;
    }

    /// <summary>Raised with what the music app reports, or null when no app has a media session.</summary>
    public event EventHandler<NowPlaying?>? NowPlayingChanged;

    /// <summary>Raised with the album art as image file bytes, or null when there is none.</summary>
    public event EventHandler<byte[]?>? ArtworkChanged;

    /// <summary>
    /// Gets the Windows media session manager. Subscribe to the events, then call ConnectAsync,
    /// so the first report is not missed.
    /// </summary>
    public static async Task<MusicMediaSession> CreateAsync()
    {
        GlobalSystemMediaTransportControlsSessionManager manager =
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        return new MusicMediaSession(manager);
    }

    /// <summary>Finds the music app and sends the first report through the events.</summary>
    public Task ConnectAsync() => ChooseSessionAsync();

    public Task<bool> TogglePlayPauseAsync() => SendAsync(session => session.TryTogglePlayPauseAsync(), "Play/pause");

    public Task<bool> NextAsync() => SendAsync(session => session.TrySkipNextAsync(), "Next");

    public Task<bool> PreviousAsync() => SendAsync(session => session.TrySkipPreviousAsync(), "Previous");

    public Task<bool> SeekAsync(TimeSpan position)
    {
        return SendAsync(session => session.TryChangePlaybackPositionAsync(position.Ticks), "Seek");
    }

    public void Dispose()
    {
        _isDisposed = true;
        _manager.SessionsChanged -= OnSessionsChanged;
        _manager.CurrentSessionChanged -= OnCurrentSessionChanged;
        StopWatchingAll();
        Detach();
        _refreshGate.Dispose();
        _chooseGate.Dispose();
    }

    private async Task<bool> SendAsync(Func<GlobalSystemMediaTransportControlsSession, IAsyncOperation<bool>> command, string name)
    {
        GlobalSystemMediaTransportControlsSession? session = _session;
        if (session is null)
        {
            return false;
        }

        try
        {
            System.Diagnostics.Trace.TraceInformation("{0} sent to {1}", name, session.SourceAppUserModelId);
            return await command(session);
        }
        catch (Exception error)
        {
            PlatformLog.Problem($"{name} command failed", error);
            return false;
        }
    }

    private void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
    {
        _ = ChooseSessionAsync();
    }

    private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        _ = ChooseSessionAsync();
    }

    // Any session starting or stopping can change which app should show (FR-41).
    private void OnAnyPlaybackChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        _ = ChooseSessionAsync();
    }

    private async Task ChooseSessionAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await _chooseGate.WaitAsync();
        try
        {
            IReadOnlyList<GlobalSystemMediaTransportControlsSession> sessions = _manager.GetSessions();
            WatchAll(sessions);

            List<MediaSessionCandidate> candidates = [.. sessions.Select(session => new MediaSessionCandidate(session.SourceAppUserModelId, IsPlaying(session)))];
            string? chosenId = MusicSource.Choose(candidates, _manager.GetCurrentSession()?.SourceAppUserModelId, _session?.SourceAppUserModelId);
            GlobalSystemMediaTransportControlsSession? chosen = sessions.FirstOrDefault(session => session.SourceAppUserModelId == chosenId);

            if (!ReferenceEquals(chosen, _session))
            {
                Detach();
                _session = chosen;
                _artworkTrackKey = null;
                _isArtworkShown = false;
                if (chosen is not null)
                {
                    chosen.MediaPropertiesChanged += OnSessionChanged;
                    chosen.TimelinePropertiesChanged += OnSessionChanged;
                }
            }

            await RefreshAsync();
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Finding the music media session failed", error);
        }
        finally
        {
            if (!_isDisposed)
            {
                _chooseGate.Release();
            }
        }
    }

    private static bool IsPlaying(GlobalSystemMediaTransportControlsSession session)
    {
        try
        {
            // Changing songs counts as playing, so a skip does not hand the island to another app.
            GlobalSystemMediaTransportControlsSessionPlaybackStatus status = session.GetPlaybackInfo().PlaybackStatus;
            return status is GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing or GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing;
        }
        catch (Exception error)
        {
            // A session that is closing can refuse; it does not play then.
            PlatformLog.Problem("Reading the state of a media session failed", error);
            return false;
        }
    }

    private void WatchAll(IReadOnlyList<GlobalSystemMediaTransportControlsSession> sessions)
    {
        StopWatchingAll();
        foreach (GlobalSystemMediaTransportControlsSession session in sessions)
        {
            session.PlaybackInfoChanged += OnAnyPlaybackChanged;
            _watched.Add(session);
        }
    }

    private void StopWatchingAll()
    {
        foreach (GlobalSystemMediaTransportControlsSession session in _watched)
        {
            session.PlaybackInfoChanged -= OnAnyPlaybackChanged;
        }

        _watched.Clear();
    }

    private void Detach()
    {
        GlobalSystemMediaTransportControlsSession? old = _session;
        if (old is null)
        {
            return;
        }

        old.MediaPropertiesChanged -= OnSessionChanged;
        old.TimelinePropertiesChanged -= OnSessionChanged;
        _session = null;
    }

    private void OnSessionChanged<TArgs>(GlobalSystemMediaTransportControlsSession sender, TArgs args)
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        // Windows can raise several events at once; read the session one refresh at a time.
        await _refreshGate.WaitAsync();
        try
        {
            GlobalSystemMediaTransportControlsSession? session = _session;
            if (session is null)
            {
                NowPlayingChanged?.Invoke(this, null);
                ArtworkChanged?.Invoke(this, null);
                return;
            }

            GlobalSystemMediaTransportControlsSessionMediaProperties properties = await session.TryGetMediaPropertiesAsync();
            NowPlayingChanged?.Invoke(this, MediaSessionReader.Read(session, properties, DateTimeOffset.Now));
            await RefreshArtworkAsync(properties);
        }
        catch (Exception error)
        {
            // The app closing mid-read is normal; report the problem and keep going.
            PlatformLog.Problem("Reading the music media session failed", error);
        }
        finally
        {
            if (!_isDisposed)
            {
                _refreshGate.Release();
            }
        }
    }

    // The picture is read again on every report, not once per song: a browser can report a new
    // video's title with the previous video's picture and send the right one a moment later (Diva,
    // YouTube in Brave, 3 Nov 2026). A new song is also checked again after 1 and 3 seconds, in case
    // the right picture arrives without an event. Only a picture that differs is passed on.
    private async Task RefreshArtworkAsync(GlobalSystemMediaTransportControlsSessionMediaProperties properties)
    {
        string trackKey = $"{properties.Title}\u0001{properties.Artist}\u0001{properties.AlbumTitle}";
        if (trackKey != _artworkTrackKey)
        {
            _artworkTrackKey = trackKey;
            _ = RetryAfterDelayAsync(LatePictureChecks[0]);
            _ = RetryAfterDelayAsync(LatePictureChecks[1]);
        }

        IRandomAccessStreamReference? thumbnail = properties.Thumbnail;
        if (thumbnail is null)
        {
            // Spotify sometimes sends the picture a moment after the title.
            ShowArtwork(null);
            RetryMissingArtwork(trackKey);
            return;
        }

        using IRandomAccessStreamWithContentType stream = await thumbnail.OpenReadAsync();
        if (stream.Size == 0)
        {
            // Not ready yet; read it again shortly.
            ShowArtwork(null);
            RetryMissingArtwork(trackKey);
            return;
        }

        if (stream.Size > MaxArtworkBytes)
        {
            ShowArtwork(null);
            return;
        }

        IBuffer buffer = await stream.ReadAsync(new global::Windows.Storage.Streams.Buffer((uint)stream.Size), (uint)stream.Size, InputStreamOptions.None);
        ShowArtwork(buffer.ToArray());
    }

    private void ShowArtwork(byte[]? picture)
    {
        bool isSame = _isArtworkShown && (picture is null ? _shownArtwork is null : _shownArtwork is not null && picture.AsSpan().SequenceEqual(_shownArtwork));
        if (isSame)
        {
            return;
        }

        _isArtworkShown = true;
        _shownArtwork = picture;
        ArtworkChanged?.Invoke(this, picture);
    }

    private void RetryMissingArtwork(string trackKey)
    {
        if (trackKey != _missingArtworkTrackKey)
        {
            _missingArtworkTrackKey = trackKey;
            _artworkRetries = 0;
        }

        if (_artworkRetries >= MaxArtworkRetries)
        {
            return;
        }

        _artworkRetries++;
        _ = RetryAfterDelayAsync(ArtworkRetryDelay);
    }

    private async Task RetryAfterDelayAsync(TimeSpan delay)
    {
        await Task.Delay(delay);
        await RefreshAsync();
    }
}
