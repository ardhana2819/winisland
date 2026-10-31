// SpotifyMediaSession.cs: connects WinIsland to the Spotify desktop app through the Windows media
// session API (Windows.Media.Control), the only music source the PRD allows. No Spotify Web API,
// no network.
//   FR-7  reads title, artist, artwork, play state, position and length.
//   FR-8  sends play/pause, next and previous.
//   FR-10 follows Spotify opening, closing and changing songs without errors.
//   FR-11 sends seek, when Spotify says it supports it (spike U6: it does).
// Windows raises its events on background threads; the app moves the results to the UI thread.

using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Media.Control;
using Windows.Storage.Streams;

using WinIsland.Core.Music;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Media;

public sealed class SpotifyMediaSession : IDisposable
{
    // Album art is small; anything bigger than this is not worth holding in memory.
    private const ulong MaxArtworkBytes = 4 * 1024 * 1024;

    private readonly GlobalSystemMediaTransportControlsSessionManager _manager;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private GlobalSystemMediaTransportControlsSession? _session;
    private string? _artworkTrackKey;
    private bool _isDisposed;

    private SpotifyMediaSession(GlobalSystemMediaTransportControlsSessionManager manager)
    {
        _manager = manager;
        _manager.SessionsChanged += OnSessionsChanged;
    }

    /// <summary>Raised with what Spotify reports, or null when Spotify has no media session.</summary>
    public event EventHandler<NowPlaying?>? NowPlayingChanged;

    /// <summary>Raised with the album art as image file bytes, or null when there is none.</summary>
    public event EventHandler<byte[]?>? ArtworkChanged;

    /// <summary>
    /// Gets the Windows media session manager. Subscribe to the events, then call ConnectAsync,
    /// so the first report is not missed.
    /// </summary>
    public static async Task<SpotifyMediaSession> CreateAsync()
    {
        GlobalSystemMediaTransportControlsSessionManager manager =
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        return new SpotifyMediaSession(manager);
    }

    /// <summary>Finds Spotify and sends the first report through the events.</summary>
    public Task ConnectAsync() => AttachToSpotifyAsync();

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
        Detach();
        _refreshGate.Dispose();
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
        _ = AttachToSpotifyAsync();
    }

    private async Task AttachToSpotifyAsync()
    {
        try
        {
            GlobalSystemMediaTransportControlsSession? spotify =
                _manager.GetSessions().FirstOrDefault(session => MusicSource.IsSpotify(session.SourceAppUserModelId));

            if (!ReferenceEquals(spotify, _session))
            {
                Detach();
                _session = spotify;
                _artworkTrackKey = null;
                if (spotify is not null)
                {
                    spotify.MediaPropertiesChanged += OnSessionChanged;
                    spotify.PlaybackInfoChanged += OnSessionChanged;
                    spotify.TimelinePropertiesChanged += OnSessionChanged;
                }
            }

            await RefreshAsync();
        }
        catch (Exception error)
        {
            PlatformLog.Problem("Finding the Spotify media session failed", error);
        }
    }

    private void Detach()
    {
        GlobalSystemMediaTransportControlsSession? old = _session;
        if (old is null)
        {
            return;
        }

        old.MediaPropertiesChanged -= OnSessionChanged;
        old.PlaybackInfoChanged -= OnSessionChanged;
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
            // Spotify closing mid-read is normal; report the problem and keep going.
            PlatformLog.Problem("Reading the Spotify media session failed", error);
        }
        finally
        {
            if (!_isDisposed)
            {
                _refreshGate.Release();
            }
        }
    }

    private async Task RefreshArtworkAsync(GlobalSystemMediaTransportControlsSessionMediaProperties properties)
    {
        string trackKey = $"{properties.Title}\u0001{properties.Artist}\u0001{properties.AlbumTitle}";
        if (trackKey == _artworkTrackKey)
        {
            return;
        }

        IRandomAccessStreamReference? thumbnail = properties.Thumbnail;
        if (thumbnail is null)
        {
            // Spotify sometimes sends the picture a moment after the title; try again on the next event.
            ArtworkChanged?.Invoke(this, null);
            return;
        }

        using IRandomAccessStreamWithContentType stream = await thumbnail.OpenReadAsync();
        if (stream.Size == 0 || stream.Size > MaxArtworkBytes)
        {
            ArtworkChanged?.Invoke(this, null);
            _artworkTrackKey = trackKey;
            return;
        }

        IBuffer buffer = await stream.ReadAsync(new global::Windows.Storage.Streams.Buffer((uint)stream.Size), (uint)stream.Size, InputStreamOptions.None);
        _artworkTrackKey = trackKey;
        ArtworkChanged?.Invoke(this, buffer.ToArray());
    }
}
