// MusicController.cs: connects the Spotify media session to the island.
// Windows reports music changes on background threads; this moves each report to the UI thread
// and hands it to the island's rules (Core), decodes the album art, and forwards the button
// presses back to Spotify.

using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;

using WinIsland.Core.Island;
using WinIsland.Core.Music;
using WinIsland.Platform.Media;

namespace WinIsland.App.Music;

internal sealed class MusicController : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly Dispatcher _dispatcher;
    private SpotifyMediaSession? _session;

    public MusicController(IslandStateMachine island, Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _island = island;
        _dispatcher = dispatcher;
    }

    /// <summary>Raised on the UI thread when the album art changes.</summary>
    public event EventHandler? ArtworkChanged;

    public ImageSource? Artwork { get; private set; }

    public async Task StartAsync()
    {
        try
        {
            _session = await SpotifyMediaSession.CreateAsync();
            // Subscribe before connecting, so the first report (what is playing right now) is not lost.
            _session.NowPlayingChanged += OnNowPlayingChanged;
            _session.ArtworkChanged += OnArtworkChanged;
            await _session.ConnectAsync();
        }
        catch (Exception error)
        {
            // Without the media session the island still works as a clock (FR-10).
            Trace.TraceWarning("The Windows media session could not be started: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
        }
    }

    public Task TogglePlayPauseAsync() => _session?.TogglePlayPauseAsync() ?? Task.FromResult(false);

    public Task NextAsync() => _session?.NextAsync() ?? Task.FromResult(false);

    public Task PreviousAsync() => _session?.PreviousAsync() ?? Task.FromResult(false);

    public Task SeekAsync(TimeSpan position) => _session?.SeekAsync(position) ?? Task.FromResult(false);

    public void Dispose()
    {
        _session?.Dispose();
    }

    private void OnNowPlayingChanged(object? sender, NowPlaying? music)
    {
        _dispatcher.BeginInvoke(() => _island.UpdateMusic(music, DateTimeOffset.Now));
    }

    private void OnArtworkChanged(object? sender, byte[]? bytes)
    {
        // Decode off the UI thread; the frozen image can then be used on the UI thread.
        ImageSource? image = ArtworkImage.FromBytes(bytes);
        _dispatcher.BeginInvoke(() =>
        {
            Artwork = image;
            ArtworkChanged?.Invoke(this, EventArgs.Empty);
        });
    }
}
