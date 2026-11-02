// GlassWindow.xaml.cs: sets up the glass test pill (spike G1 to G5).
//   1. The window never takes keyboard focus, like the island (FR-3).
//   2. Windows draws the blurred backdrop: the frame is extended over the whole window and the
//      system backdrop type is set (acrylic by default). The window background stays clear so
//      the backdrop shows through.
//   3. Every frame while the pill changes shape, the window region is set to the pill's rounded
//      rectangle, so the blur only shows inside the pill. Regions have hard edges (no smoothing):
//      part of what this spike checks is whether that is visible.
// Options:
//   --backdrop acrylic|mica|tabbed|none   which Windows backdrop to ask for (default acrylic)
//   --tint 0.45                           how dark the tint over the blur is, 0 to 1 (default 0.45)
//   --gpu                                 draw with the graphics card; default is the CPU, like WinIsland
//   --keep-active                         tell Windows the frame is always active (WM_NCACTIVATE), so the
//                                         backdrop does not turn flat grey when another window has focus.
//                                         Keyboard focus is not taken; only the frame's look changes.
//   --accent acrylic|blur                 use the older SetWindowCompositionAttribute blur instead of the
//                                         system backdrop. It does not depend on the window being active.

using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GlassProbe;

public partial class GlassWindow : Window
{
    private const double TopMarginDips = 12;
    private const double CompactWidth = 120, CompactHeight = 34, CompactRadius = 17;
    private const double ExpandedWidth = 420, ExpandedHeight = 190, ExpandedRadius = 28;
    private const double ShapeSeconds = 0.6;

    private readonly string _backdrop;
    private readonly double _tint;
    private readonly bool _gpu;
    private readonly bool _keepActive;
    private readonly string? _accent;
    private IntPtr _handle;
    private bool _expanded;
    private Size _regionSize;
    private string _dwmResult = "not set";

    public GlassWindow()
    {
        string[] args = Environment.GetCommandLineArgs();
        _backdrop = ValueAfter(args, "--backdrop") ?? "acrylic";
        _tint = double.TryParse(ValueAfter(args, "--tint"), NumberStyles.Float, CultureInfo.InvariantCulture, out double tint) ? Math.Clamp(tint, 0, 1) : 0.45;
        _gpu = args.Contains("--gpu");
        _keepActive = args.Contains("--keep-active");
        _accent = ValueAfter(args, "--accent");
        if (!_gpu)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        }

        InitializeComponent();
        Pill.Background = new SolidColorBrush(Color.FromArgb((byte)Math.Round(_tint * 255), 0, 0, 0));
        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _handle = new WindowInteropHelper(this).Handle;
        HwndSource source = HwndSource.FromHwnd(_handle);
        source.AddHook(RefuseMouseActivation);
        if (_keepActive)
        {
            source.AddHook(KeepFrameActive);
            // The window is shown without being activated, so Windows never says "active" by itself.
            ContentRendered += (_, _) => NativeMethods.DefWindowProc(_handle, NativeMethods.WmNcActivate, new IntPtr(1), IntPtr.Zero);
        }

        source.CompositionTarget.BackgroundColor = Colors.Transparent;

        long style = NativeMethods.GetWindowLongPtr(_handle, NativeMethods.GwlExStyle).ToInt64();
        NativeMethods.SetWindowLongPtr(_handle, NativeMethods.GwlExStyle, new IntPtr(style | NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow));

        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = TopMarginDips;

        if (_accent is null)
        {
            ApplyBackdrop();
        }
        else
        {
            ApplyAccent();
        }

        // Set the shape once before the first frame too, so the window never shows as a full rectangle.
        UpdateLayout();
        UpdateRegion();
        CompositionTarget.Rendering += (_, _) => UpdateRegion();
        ShowStatus();
    }

    private void ApplyBackdrop()
    {
        NativeMethods.Margins wholeWindow = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        int extend = NativeMethods.DwmExtendFrameIntoClientArea(_handle, ref wholeWindow);

        int dark = 1;
        NativeMethods.DwmSetWindowAttribute(_handle, NativeMethods.DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));
        int noRounding = NativeMethods.DwmwcpDoNotRound;
        NativeMethods.DwmSetWindowAttribute(_handle, NativeMethods.DwmwaWindowCornerPreference, ref noRounding, sizeof(int));

        // DWMSBT values: 1 none, 2 mica, 3 acrylic ("transient window"), 4 tabbed.
        int type = _backdrop switch
        {
            "none" => 1,
            "mica" => 2,
            "tabbed" => 4,
            _ => 3,
        };
        int backdrop = NativeMethods.DwmSetWindowAttribute(_handle, NativeMethods.DwmwaSystemBackdropType, ref type, sizeof(int));
        _dwmResult = $"extend 0x{extend:X8}, backdrop 0x{backdrop:X8}";
    }

    private void ApplyAccent()
    {
        NativeMethods.Margins wholeWindow = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        NativeMethods.DwmExtendFrameIntoClientArea(_handle, ref wholeWindow);
        int noRounding = NativeMethods.DwmwcpDoNotRound;
        NativeMethods.DwmSetWindowAttribute(_handle, NativeMethods.DwmwaWindowCornerPreference, ref noRounding, sizeof(int));

        NativeMethods.AccentPolicy accent = new()
        {
            AccentState = _accent == "blur" ? NativeMethods.AccentEnableBlurBehind : NativeMethods.AccentEnableAcrylicBlurBehind,
            // Acrylic needs a little colour of its own or it turns solid; the real tint is the pill's brush.
            GradientColor = 0x10000000,
        };
        int size = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.AccentPolicy>();
        IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
        try
        {
            System.Runtime.InteropServices.Marshal.StructureToPtr(accent, buffer, false);
            NativeMethods.WindowCompositionAttributeData data = new() { Attribute = NativeMethods.WcaAccentPolicy, Data = buffer, SizeOfData = size };
            int result = NativeMethods.SetWindowCompositionAttribute(_handle, ref data);
            _dwmResult = $"accent {_accent}, result {result}";
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
        }
    }

    // Keeps the window region the same as the pill, so the blur shows only inside it.
    private void UpdateRegion()
    {
        Size size = new(Pill.ActualWidth, Pill.ActualHeight);
        if (size == _regionSize || size.Width <= 0)
        {
            return;
        }

        _regionSize = size;
        double progress = Math.Clamp((size.Height - CompactHeight) / (ExpandedHeight - CompactHeight), 0, 1.2);
        double radius = CompactRadius + (progress * (ExpandedRadius - CompactRadius));
        Pill.CornerRadius = new CornerRadius(radius);

        DpiScale dpi = VisualTreeHelper.GetDpi(this);
        // The window's fixed Width, not ActualWidth: ActualWidth is still 0 before the first show,
        // which put the first region off to the left and left the whole window visible.
        int left = (int)Math.Round((Width - size.Width) / 2 * dpi.DpiScaleX);
        int right = (int)Math.Round((Width + size.Width) / 2 * dpi.DpiScaleX);
        int bottom = (int)Math.Round(size.Height * dpi.DpiScaleY);
        int corner = (int)Math.Round(radius * 2 * dpi.DpiScaleX);
        NativeMethods.SetWindowRgn(_handle, NativeMethods.CreateRoundRectRgn(left, 0, right + 1, bottom + 1, corner, corner), true);
    }

    private void OnPillClicked(object sender, MouseButtonEventArgs e)
    {
        _expanded = !_expanded;
        // A small overshoot, close to the island's spring.
        BackEase spring = new() { Amplitude = 0.3, EasingMode = EasingMode.EaseOut };
        Pill.BeginAnimation(WidthProperty, new DoubleAnimation(_expanded ? ExpandedWidth : CompactWidth, TimeSpan.FromSeconds(ShapeSeconds)) { EasingFunction = spring });
        Pill.BeginAnimation(HeightProperty, new DoubleAnimation(_expanded ? ExpandedHeight : CompactHeight, TimeSpan.FromSeconds(ShapeSeconds)) { EasingFunction = spring });
        StatusText.Opacity = _expanded ? 1 : 0;
        ClockText.VerticalAlignment = _expanded ? VerticalAlignment.Top : VerticalAlignment.Center;
        ClockText.Margin = _expanded ? new Thickness(0, 16, 0, 0) : new Thickness(0);
    }

    private void OnPillRightClicked(object sender, MouseButtonEventArgs e) => Close();

    private void ShowStatus()
    {
        string mode = _accent is null ? $"backdrop {_backdrop}" : $"accent {_accent}";
        StatusText.Text = $"Mode: {mode}{(_keepActive ? ", keep-active" : "")}, tint {_tint:0.00}, drawing: {(_gpu ? "graphics card" : "CPU")}.\nDWM: {_dwmResult}. Click to shrink, right-click to close.";
    }

    // Windows draws the system backdrop only while the frame looks active. The island is never the
    // active window, so this answers "stay active" for the frame's look. Keyboard focus is unchanged.
    private static IntPtr KeepFrameActive(IntPtr window, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == NativeMethods.WmNcActivate && wParam == IntPtr.Zero)
        {
            handled = true;
            return NativeMethods.DefWindowProc(window, message, new IntPtr(1), lParam);
        }

        return IntPtr.Zero;
    }

    private static IntPtr RefuseMouseActivation(IntPtr window, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == NativeMethods.WmMouseActivate)
        {
            handled = true;
            return new IntPtr(NativeMethods.MaNoActivate);
        }

        return IntPtr.Zero;
    }

    private static string? ValueAfter(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
