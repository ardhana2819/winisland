// NotificationBannerView.xaml.cs: fills the banner and turns mouse input into tap or swipe.
// Tap (no real sideways movement) opens the notifications page (FR-15, FR-21). A sideways drag of more than 40
// slides the banner 90 to that side while it fades, over 0.2 s (DESIGN_SPEC section 6), then
// reports the swipe so the island shows the next waiting banner.
// v1.4 (FR-38): as the call banner it shows the caller and the call circle, and a tap does
// nothing, so a click during a game cannot answer or open anything; a swipe still hides it.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;
using WinIsland.App.Text;
using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class NotificationBannerView : UserControl
{
    private const double SwipeThresholdDips = 40;
    private const double SwipeDistanceDips = 90;
    private const double SwipeSeconds = 0.2;

    private Point? _pressedAt;
    private bool _isSwiping;
    private CallSymbol? _callSymbol;
    private bool _isCall;

    public NotificationBannerView()
    {
        InitializeComponent();
    }

    public event EventHandler? Tapped;

    public event EventHandler? Swiped;

    public void Show(IslandNotification notification, string preview, int waitingCount, ImageSource? logo)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Icon.Show(notification, logo);
        EmojiText.Show(SenderText, notification.Sender);
        EmojiText.Show(PreviewText, preview);
        PreviewText.Visibility = string.IsNullOrEmpty(preview) ? Visibility.Collapsed : Visibility.Visible;

        // +N counts the other waiting banners; hidden when this is the only one.
        int others = waitingCount - 1;
        Badge.Visibility = others > 0 ? Visibility.Visible : Visibility.Collapsed;
        BadgeText.Text = $"+{others}";
    }

    /// <summary>Shows a ringing call instead of a message (FR-38).</summary>
    public void ShowCall(IncomingCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        _isCall = true;
        Cursor = Cursors.Arrow;
        Icon.Show(call.Notification, null);
        EmojiText.Show(SenderText, call.Caller);
        EmojiText.Show(PreviewText, call.Description);
        PreviewText.Visibility = Visibility.Visible;
        Badge.Visibility = Visibility.Collapsed;

        if (_callSymbol is null)
        {
            _callSymbol = new CallSymbol(36, 20) { VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(_callSymbol, 2);
            Row.Children.Add(_callSymbol);
        }

        _callSymbol.ShowKind(call.IsVideo);
    }

    /// <summary>The call circle pulses only while the call banner is on screen.</summary>
    public void SetPulsing(bool isPulsing)
    {
        if (isPulsing)
        {
            _callSymbol?.Start();
        }
        else
        {
            _callSymbol?.Stop();
        }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (_isSwiping)
        {
            return;
        }

        _pressedAt = e.GetPosition(this);
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_pressedAt is not Point start || _isSwiping)
        {
            return;
        }

        // Follow the finger a little, so the drag feels connected before it counts as a swipe.
        SwipeOffset.X = e.GetPosition(this).X - start.X;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (_pressedAt is not Point start || _isSwiping)
        {
            return;
        }

        ReleaseMouseCapture();
        _pressedAt = null;
        double moved = e.GetPosition(this).X - start.X;

        if (Math.Abs(moved) <= SwipeThresholdDips)
        {
            SwipeOffset.X = 0;
            if (!_isCall)
            {
                Tapped?.Invoke(this, EventArgs.Empty);
            }

            return;
        }

        SwipeAway(Math.Sign(moved));
    }

    private void SwipeAway(int direction)
    {
        if (!Motion.IsEnabled)
        {
            FinishSwipe();
            return;
        }

        _isSwiping = true;
        IEasingFunction ease = CubicBezierEase.Ease();
        BannerContent.BeginAnimation(OpacityProperty, Motion.To(0, SwipeSeconds, ease));
        DoubleAnimation slide = Motion.To(direction * SwipeDistanceDips, SwipeSeconds, ease).Clone();
        slide.Completed += (_, _) => FinishSwipe();
        SwipeOffset.BeginAnimation(TranslateTransform.XProperty, slide);
    }

    private void FinishSwipe()
    {
        _isSwiping = false;
        // Put the content back, ready for the next banner.
        BannerContent.BeginAnimation(OpacityProperty, null);
        SwipeOffset.BeginAnimation(TranslateTransform.XProperty, null);
        BannerContent.Opacity = 1;
        SwipeOffset.X = 0;
        Swiped?.Invoke(this, EventArgs.Empty);
    }
}
