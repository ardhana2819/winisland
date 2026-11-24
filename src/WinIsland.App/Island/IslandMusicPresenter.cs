// IslandMusicPresenter.cs: keeps the music views on the island up to date.
// Fills the compact music pill and the expanded controls from the island's state (or the empty
// view when there is no music session, FR-24) with the artwork's colors (FR-25; v1.9, FR-48: also
// the pill's equalizer bars), moves the
// progress on once per second while the expanded island shows playing music (DESIGN_SPEC
// section 7; nothing ticks otherwise), and sends the button presses to the music app (FR-8, FR-11).
// v1.5 (FR-39): the speaker button and Ctrl+Up or Ctrl+Down show the music app's volume in place
// of the progress row; it goes back 3 seconds after the last touch, on a second click, or when
// the music page is no longer on screen.

using System.Windows.Threading;

using WinIsland.App.Music;
using WinIsland.Core.Island;
using WinIsland.Core.Music;

namespace WinIsland.App.Island;

internal sealed class IslandMusicPresenter : IDisposable
{
    private static readonly TimeSpan ProgressInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan VolumeShownFor = TimeSpan.FromSeconds(3);
    private const float VolumeStep = 0.1f;
    private const float WheelStep = 0.05f;

    private readonly IslandStateMachine _island;
    private readonly MusicController _music;
    private readonly MusicPillView _pill;
    private readonly MusicControlsView _controls;
    private readonly DispatcherTimer _progressTimer;
    private readonly DispatcherTimer _volumeTimer;
    private bool _isVolumeShown;

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

        _volumeTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = VolumeShownFor };
        _volumeTimer.Tick += (_, _) => HideVolume();
        _controls.SpeakerClicked += (_, _) =>
        {
            if (_isVolumeShown)
            {
                HideVolume();
            }
            else
            {
                ShowVolume(_music.GetVolume());
            }
        };
        _controls.VolumeWheeled += (_, notches) => ChangeVolume(notches * WheelStep);
        _controls.VolumeChosen += (_, level) =>
        {
            _music.SetVolume(level);
            ShowVolume(level);
        };
    }

    /// <summary>Ctrl+Up or Ctrl+Down while the island is open: one step louder or quieter (FR-39).</summary>
    public void StepVolume(bool louder) => ChangeVolume(louder ? VolumeStep : -VolumeStep);

    // Ctrl+Up or Ctrl+Down (10 percent) and the wheel over the volume (5 percent a notch, FR-39).
    private void ChangeVolume(float change)
    {
        if (_island.Music is null || _music.GetVolume() is not float level)
        {
            return;
        }

        float next = Math.Clamp(level + change, 0f, 1f);
        _music.SetVolume(next);
        ShowVolume(next);
    }

    // Shows the volume row and (re)starts its 3 seconds.
    private void ShowVolume(float? level)
    {
        _isVolumeShown = true;
        _controls.ShowVolume(true, level);
        _volumeTimer.Stop();
        _volumeTimer.Start();
    }

    private void HideVolume()
    {
        _volumeTimer.Stop();
        if (!_isVolumeShown)
        {
            return;
        }

        _isVolumeShown = false;
        _controls.ShowVolume(false, null);
    }

    /// <summary>
    /// The music page's colors (FR-25): from the artwork while a song with artwork is reported,
    /// otherwise the black look (no artwork, or the empty view).
    /// </summary>
    public ArtworkPalette CurrentPalette =>
        _island.Music is not null && _music.Artwork is not null ? _music.Palette : ArtworkPalette.Black;

    /// <summary>The look changed (FR-37, FR-45): on Glass the music page's second lines are near-white.</summary>
    public void SetGlass(bool isGlass) => _controls.SetGlass(isGlass);

    /// <summary>Call after every island change.</summary>
    public void Refresh()
    {
        NowPlaying? music = _island.Music;
        if (music is not null)
        {
            _pill.Show(music, _music.Artwork);
            _pill.SetEqualizerColor(EqualizerColor.ForPill(CurrentPalette, hasArtwork: _music.Artwork is not null));
            _controls.Show(music, _music.Artwork, DateTimeOffset.Now);
        }
        else
        {
            // No music session: the expanded island shows the empty view (FR-24).
            _controls.ShowEmpty();
        }

        _controls.ApplyPalette(CurrentPalette);

        // Only the music page on screen animates and ticks; the notifications page hides it.
        bool musicPageShown = _island.IsVisible && _island.View == IslandView.Expanded && _island.Page == ExpandedPage.Music;
        _pill.SetActive(_island.IsVisible && _island.View == IslandView.Music);
        _controls.SetActive(musicPageShown);
        if (!musicPageShown)
        {
            HideVolume();
        }

        bool needsTicks = music is { IsPlaying: true } && musicPageShown;
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
        _volumeTimer.Stop();
    }
}
