// SettingsWindow.xaml.cs: shows the current settings and sends each change to SettingsController,
// which checks, applies and saves it at once (no Save button needed).
// Changing a hotkey: click its button, then press the new combination (Escape cancels). While
// recording, WinIsland frees its own hotkeys so the combination reaches this window. A refused
// change (a rule in Core, or a combination another app owns) shows in amber under the hotkeys,
// like the design board.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Settings;
using WinIsland.Platform.Windows;

namespace WinIsland.App.Settings;

public partial class SettingsWindow : Window
{
    private const string RecordingText = "Press keys…";

    private readonly SettingsController _controller;
    private Button? _recordingButton;
    private bool _isShowingSettings;

    internal SettingsWindow(SettingsController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        InitializeComponent();

        int min = (int)AppSettings.MinBannerDuration.TotalSeconds;
        int max = (int)AppSettings.MaxBannerDuration.TotalSeconds;
        for (int seconds = min; seconds <= max; seconds++)
        {
            DurationBox.Items.Add(new ComboBoxItem { Content = $"{seconds} seconds", Tag = seconds });
        }

        SourceInitialized += (_, _) => DarkTitleBar.Apply(new WindowInteropHelper(this).Handle);
        PreviewKeyDown += OnPreviewKeyDown;
        Deactivated += (_, _) => StopRecording(applied: false);
        Closed += (_, _) => StopRecording(applied: false);
        _controller.Changed += (_, _) => ShowSettings();
        ShowSettings();
        ShowProblems(_controller.HotkeyConflictText is null ? [] : [$"{_controller.HotkeyConflictText}. Choose a different shortcut."]);
    }

    private void ShowSettings()
    {
        _isShowingSettings = true;
        AppSettings settings = _controller.Current;

        ExpandHotkeyButton.Content = Spaced(settings.Hotkeys.Expand);
        HideShowHotkeyButton.Content = Spaced(settings.Hotkeys.HideShow);
        HideNotificationsHotkeyButton.Content = Spaced(settings.Hotkeys.HideBanner);

        int seconds = (int)settings.Notifications.BannerDuration.TotalSeconds;
        DurationBox.SelectedItem = DurationBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (int)item.Tag == seconds);

        WhatsAppSwitch.IsChecked = settings.Notifications.ShowWhatsApp;
        DiscordSwitch.IsChecked = settings.Notifications.ShowDiscord;
        OtherAppsSwitch.IsChecked = settings.Notifications.ShowOtherApps;
        HideTextSwitch.IsChecked = settings.Notifications.HideMessageText;
        StartupSwitch.IsChecked = settings.LaunchAtStartup;
        _isShowingSettings = false;
    }

    private void OnSwitchClick(object sender, RoutedEventArgs e)
    {
        AppSettings current = _controller.Current;
        Change(current with
        {
            Notifications = current.Notifications with
            {
                ShowWhatsApp = WhatsAppSwitch.IsChecked == true,
                ShowDiscord = DiscordSwitch.IsChecked == true,
                ShowOtherApps = OtherAppsSwitch.IsChecked == true,
                HideMessageText = HideTextSwitch.IsChecked == true,
            },
            LaunchAtStartup = StartupSwitch.IsChecked == true,
        });
    }

    private void OnDurationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isShowingSettings || DurationBox.SelectedItem is not ComboBoxItem { Tag: int seconds })
        {
            return;
        }

        AppSettings current = _controller.Current;
        Change(current with
        {
            Notifications = current.Notifications with { BannerDuration = TimeSpan.FromSeconds(seconds) },
        });
    }

    private void OnHotkeyButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || _recordingButton is not null)
        {
            return;
        }

        _recordingButton = button;
        button.Content = RecordingText;
        _controller.PauseHotkeys();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_recordingButton is not Button button)
        {
            return;
        }

        e.Handled = true;
        if (e.Key == Key.Escape)
        {
            StopRecording(applied: false);
            return;
        }

        Hotkey? pressed = HotkeyCapture.FromKeyPress(e);
        if (pressed is null)
        {
            // Only a modifier so far (or a key that cannot be a hotkey): keep waiting.
            return;
        }

        HotkeySet hotkeys = _controller.Current.Hotkeys;
        HotkeySet changed = button == ExpandHotkeyButton ? hotkeys with { Expand = pressed }
            : button == HideShowHotkeyButton ? hotkeys with { HideShow = pressed }
            : hotkeys with { HideBanner = pressed };

        bool applied = Change(_controller.Current with { Hotkeys = changed });
        StopRecording(applied);
    }

    private void StopRecording(bool applied)
    {
        if (_recordingButton is null)
        {
            return;
        }

        _recordingButton = null;
        if (!applied)
        {
            // Take back the hotkeys that were freed for recording.
            _controller.ResumeHotkeys();
        }

        ShowSettings();
    }

    private bool Change(AppSettings next)
    {
        IReadOnlyList<string> problems = _controller.TryChange(next);
        List<string> shown = [.. problems];
        if (_controller.HotkeyConflictText is string conflict)
        {
            shown.Add($"{conflict}. Choose a different shortcut.");
        }

        ShowProblems(shown);
        ShowSettings();
        return problems.Count == 0;
    }

    private void ShowProblems(IReadOnlyList<string> problems)
    {
        ProblemText.Text = string.Join(Environment.NewLine, problems);
        ProblemText.Visibility = problems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // Design board writes hotkeys with spaces: "Ctrl + Alt + I".
    private static string Spaced(Hotkey hotkey) => hotkey.ToString().Replace("+", " + ", StringComparison.Ordinal);
}
