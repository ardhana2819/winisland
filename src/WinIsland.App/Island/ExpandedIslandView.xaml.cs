// ExpandedIslandView.xaml.cs: slides between the music and notifications pages and draws the
// page dots (PRD FR-21, DESIGN_SPEC section 9).
//   Page switch: the track moves 190 over 0.5 s with the softer page spring.
//   Dots: the current one grows from 6 to 14 high over 0.3 s. v1.3: they show on opening and
//   on a page change and fade out 1.5 s later (FR-31, PageDotsFader).
// With Windows animation effects off, both happen at once. A dot click is reported; like the
// music buttons, a dot is a Button, which keeps the click from also closing the island.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;
using WinIsland.App.Music;
using WinIsland.App.Notifications;
using WinIsland.Core.Island;

namespace WinIsland.App.Island;

public partial class ExpandedIslandView : UserControl
{
    private const double PageHeightDips = 190;
    private const double SlideSeconds = 0.5;
    private const double DotSeconds = 0.3;
    private const double SmallDotDips = 6;
    private const double CurrentDotDips = 14;

    private static readonly IEasingFunction SlideCurve = CubicBezierEase.PageSlide();
    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();
    private static readonly Brush CurrentDotBrush = Frozen(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
    private static readonly Brush OtherDotBrush = Frozen(Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF));

    private readonly PageDotsFader _dotsFader;
    private ExpandedPage _shownPage = ExpandedPage.Music;

    public ExpandedIslandView()
    {
        InitializeComponent();
        _dotsFader = new PageDotsFader(Dots);
    }

    /// <summary>A page dot was clicked.</summary>
    public event EventHandler<ExpandedPage>? PageRequested;

    public MusicControlsView Music => MusicPage;

    public NotificationsPageView Notifications => NotificationsPage;

    /// <summary>The island just opened: show the dots so the second page is easy to find (FR-31).</summary>
    public void OnOpened() => _dotsFader.Flash();

    /// <summary>The island closed: the dots hide at once, ready to fade in on the next opening.</summary>
    public void OnClosed() => _dotsFader.HideNow();

    /// <summary>
    /// Shows a page. It slides only when the island is already open on screen (slide is true);
    /// opening straight onto a page, for example from a banner, shows it at once.
    /// </summary>
    public void ShowPage(ExpandedPage page, bool slide)
    {
        if (page == _shownPage)
        {
            return;
        }

        _shownPage = page;
        bool onNotifications = page == ExpandedPage.Notifications;
        Move(TrackOffset, TranslateTransform.YProperty, onNotifications ? -PageHeightDips : 0, SlideSeconds, SlideCurve, slide);
        Move(MusicDot, HeightProperty, onNotifications ? SmallDotDips : CurrentDotDips, DotSeconds, EaseCurve, slide);
        Move(NotificationsDot, HeightProperty, onNotifications ? CurrentDotDips : SmallDotDips, DotSeconds, EaseCurve, slide);
        MusicDot.Background = onNotifications ? OtherDotBrush : CurrentDotBrush;
        NotificationsDot.Background = onNotifications ? CurrentDotBrush : OtherDotBrush;
        if (slide)
        {
            _dotsFader.Flash();
        }
    }

    private void OnMusicDotClick(object sender, RoutedEventArgs e) => PageRequested?.Invoke(this, ExpandedPage.Music);

    private void OnNotificationsDotClick(object sender, RoutedEventArgs e) => PageRequested?.Invoke(this, ExpandedPage.Notifications);

    private static void Move(IAnimatable target, DependencyProperty property, double to, double seconds, IEasingFunction curve, bool animate)
    {
        if (animate)
        {
            Motion.Run(target, property, Motion.To(to, seconds, curve));
            return;
        }

        target.BeginAnimation(property, null);
        ((DependencyObject)target).SetValue(property, to);
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
