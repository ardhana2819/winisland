// MusicController.cs: connects the music app's media session to the island (v1.5: any app, FR-41).
// Windows reports music changes on background threads; this moves each report to the UI thread
// and hands it to the island's rules (Core), decodes the album art and works out its colors
// (FR-25), forwards the button presses back to the music app, and reads and sets the music app's
// volume (FR-39).

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
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(30);

    private readonly CancellationTokenSource _stop = new();
    private MusicMediaSession? _session;

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

    /// <summary>The music page's colors from the album art; Black without a picture.</summary>
    public ArtworkPalette Palette { get; private set; } = ArtworkPalette.Black;

    /// <summary>
    /// Connects to the Windows media session. If Windows refuses (rare, for example right after
    /// sign-in), it tries again every 30 seconds instead of giving up (PRD section 7: recover).
    /// Without it the island still works as a clock (FR-10).
    /// </summary>
    public async Task StartAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                MusicMediaSession session = await MusicMediaSession.CreateAsync();
                // Subscribe before connecting, so the first report (what is playing right now) is not lost.
                session.NowPlayingChanged += OnNowPlayingChanged;
                session.ArtworkChanged += OnArtworkChanged;
                await session.ConnectAsync();
                _session = session;
                return;
            }
            catch (Exception error)
            {
                Trace.TraceWarning("The Windows media session could not be started, trying again in 30 s: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
            }

            try
            {
                await Task.Delay(RetryInterval, _stop.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    public Task TogglePlayPauseAsync() => _session?.TogglePlayPauseAsync() ?? Task.FromResult(false);

    public Task NextAsync() => _session?.NextAsync() ?? Task.FromResult(false);

    public Task PreviousAsync() => _session?.PreviousAsync() ?? Task.FromResult(false);

    public Task SeekAsync(TimeSpan position) => _session?.SeekAsync(position) ?? Task.FromResult(false);

    /// <summary>The music app's volume, 0 to 1, or null when it has no sound open with Windows (FR-39).</summary>
    public float? GetVolume() => _island.Music is NowPlaying music ? AppVolume.Get(music.AppId) : null;

    /// <summary>Sets the music app's volume, 0 to 1; Windows' own volume and other apps stay as they are.</summary>
    public void SetVolume(float level)
    {
        if (_island.Music is NowPlaying music && !AppVolume.Set(music.AppId, level))
        {
            Trace.TraceInformation("The music app has no sound open with Windows; volume not set");
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _session?.Dispose();
        _stop.Dispose();
    }

    private void OnNowPlayingChanged(object? sender, NowPlaying? music)
    {
        _dispatcher.BeginInvoke(() => _island.UpdateMusic(music, DateTimeOffset.Now));
    }

    private void OnArtworkChanged(object? sender, byte[]? bytes)
    {
        // Decode and work out the colors off the UI thread; the frozen image can then be used on it.
        ImageSource? image = ArtworkImage.FromBytes(bytes);
        ArtworkPalette palette = image is null ? ArtworkPalette.Black : ArtworkColors.FromBytes(bytes);
        _dispatcher.BeginInvoke(() =>
        {
            Artwork = image;
            Palette = palette;
            ArtworkChanged?.Invoke(this, EventArgs.Empty);
        });
    }
}
