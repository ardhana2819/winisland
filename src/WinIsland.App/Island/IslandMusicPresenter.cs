// IslandMusicPresenter.cs: keeps the music views on the island up to date.
// Fills the compact music pill and the expanded controls from the island's state, moves the
// progress on once per second while the expanded island shows playing music (DESIGN_SPEC
// section 7; nothing ticks otherwise), and sends the button presses to Spotify (FR-8, FR-11).

using System.Windows.Threading;

using WinIsland.App.Music;
using WinIsland.Core.Island;
using WinIsland.Core.Music;

namespace WinIsland.App.Island;

internal sealed class IslandMusicPresenter : IDisposable
{
    private static readonly TimeSpan ProgressInterval = TimeSpan.FromSeconds(1);

    private readonly IslandStateMachine _island;
    private readonly MusicController _music;
    private readonly MusicPillView _pill;
    private readonly MusicControlsView _controls;
    private readonly DispatcherTimer _progressTimer;

    public IslandMusicPresenter(IslandStateMachine island, MusicController music, MusicPillView pill, MusicControlsView controls)
    {
        _island = island;
        _music = music;
        _pill = pill;
        _controls = controls;

        _progressTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = ProgressInterval };
        _progressTimer.Tick += (_, _) => _controls.UpdateProgress(DateTimeOffset.Now);

        _music.ArtworkChanged += (_, _) => Refresh();
        _controls.PreviousClicked += (_, _) => _ = _music.PreviousAsync();
        _controls.PlayPauseClicked += (_, _) => _ = _music.TogglePlayPauseAsync();
        _controls.NextClicked += (_, _) => _ = _music.NextAsync();
        _controls.SeekRequested += (_, position) => _ = _music.SeekAsync(position);
    }

    /// <summary>Call after every island change.</summary>
    public void Refresh()
    {
        NowPlaying? music = _island.Music;
        if (music is not null)
        {
            _pill.Show(music, _music.Artwork);
            _controls.Show(music, _music.Artwork, DateTimeOffset.Now);
        }

        bool needsTicks = music is { IsPlaying: true } && _island.View == IslandView.Expanded;
        if (needsTicks && !_progressTimer.IsEnabled)
        {
            _progressTimer.Start();
        }
        else if (!needsTicks && _progressTimer.IsEnabled)
        {
            _progressTimer.Stop();
        }
    }

    public void Dispose()
    {
        _progressTimer.Stop();
    }
}
