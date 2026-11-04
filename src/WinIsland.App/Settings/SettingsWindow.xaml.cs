// SettingsWindow.xaml.cs: shows the current settings and sends each change to SettingsController,
// which checks, applies and saves it at once (no Save button needed).
// Changing a hotkey: click its button, then press the new combination (Escape cancels). While
// recording, WinIsland frees its own hotkeys so the combination reaches this window. A refused
// change (a rule in Core, or a combination another app owns) shows in amber under the hotkeys,
// like the design board.
// Island size (FR-32): Small, Medium, Large or Extra large; the island resizes at once.
// v1.7 (FR-46): the installed version under the title, and an Update button that starts the
// update script in its own window. WinIsland itself does not go online. When the window opens,
// the script's quiet check runs without a window; only if a newer release exists does an amber
// "Update available: x" show under the button (Diva, 4 Nov 2026).

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Core.Settings;
using WinIsland.Core.Updates;
using WinIsland.Platform.Updates;
using WinIsland.Platform.Windows;

namespace WinIsland.App.Settings;

public partial class SettingsWindow : Window
{
    private const string RecordingText = "Press keys…";
    private static readonly TimeSpan UpdateCheckTimeout = TimeSpan.FromSeconds(20);

    private static readonly (IslandSize Size, string Name)[] SizeNames =
    [
        (IslandSize.Small, "Small"),
        (IslandSize.Medium, "Medium"),
        (IslandSize.Large, "Large"),
        (IslandSize.ExtraLarge, "Extra large"),
    ];

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

        foreach ((IslandSize size, string name) in SizeNames)
        {
            SizeBox.Items.Add(new ComboBoxItem { Content = name, Tag = size });
        }

        LookBox.Items.Add(new ComboBoxItem { Content = "Solid", Tag = IslandLook.Solid });
        // Diva, 2 Nov 2026: Glass stays marked beta, also after the release, so anyone choosing it
        // knows it may not look right in every game.
        LookBox.Items.Add(new ComboBoxItem { Content = "Glass (beta)", Tag = IslandLook.Glass });

        ShowVersionAndUpdate();
        SourceInitialized += (_, _) => DarkTitleBar.Apply(new WindowInteropHelper(this).Handle);
        PreviewKeyDown += OnPreviewKeyDown;
        Deactivated += (_, _) => StopRecording(applied: false);
        Closed += (_, _) => StopRecording(applied: false);
        _controller.Changed += (_, _) => ShowSettings();
        ShowSettings();
        ShowProblems(_controller.HotkeyConflictText is null ? [] : [$"{_controller.HotkeyConflictText}. Choose a different shortcut."]);
    }

    // The informational version keeps the "-beta.1" part ("1.7.0-beta.1"); the assembly version cannot.
    private void ShowVersionAndUpdate()
    {
        string version = typeof(SettingsWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        VersionText.Text = $"Version {version}";
        bool canUpdate = UpdateScript.Find() is not null;
        UpdateButton.IsEnabled = canUpdate;
        UpdateButton.ToolTip = canUpdate ? "Install the newest WinIsland" : "Update needs the installed WinIsland";
        if (canUpdate)
        {
            Loaded += async (_, _) => await ShowIfUpdateAvailableAsync();
        }
    }

    // Each opening checks again; up to date, offline or no GitHub CLI shows nothing.
    private async Task ShowIfUpdateAvailableAsync()
    {
        if (UpdateScript.Find() is not string script)
        {
            return;
        }

        string? version = UpdateCheckOutput.AvailableVersion(await UpdateScript.CheckAsync(script, UpdateCheckTimeout));
        UpdateNote.Text = version is null ? string.Empty : $"Update available: {version}";
        UpdateNote.Visibility = version is null ? Visibility.Hidden : Visibility.Visible;
    }

    private void OnUpdateClick(object sender, RoutedEventArgs e)
    {
        if (UpdateScript.Find() is not string script || !UpdateScript.Start(script))
        {
            ShowProblems(["The update could not start. Use Start > Update WinIsland, or the setup from the Releases page."]);
        }
    }

    private void ShowSettings()
    {
        _isShowingSettings = true;
        AppSettings settings = _controller.Current;

        ExpandHotkeyButton.Content = Spaced(settings.Hotkeys.Expand);
        HideShowHotkeyButton.Content = Spaced(settings.Hotkeys.HideShow);
        HideNotificationsHotkeyButton.Content = Spaced(settings.Hotkeys.HideBanner);

        SizeBox.SelectedItem = SizeBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (IslandSize)item.Tag == settings.Size);
        LookBox.SelectedItem = LookBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (IslandLook)item.Tag == settings.Look);

        int seconds = (int)settings.Notifications.BannerDuration.TotalSeconds;
        DurationBox.SelectedItem = DurationBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (int)item.Tag == seconds);

        WhatsAppSwitch.IsChecked = settings.Notifications.ShowWhatsApp;
        DiscordSwitch.IsChecked = settings.Notifications.ShowDiscord;
        OtherAppsSwitch.IsChecked = settings.Notifications.ShowOtherApps;
        HideTextSwitch.IsChecked = settings.Notifications.HideMessageText;
        ClearFromWindowsSwitch.IsChecked = settings.Notifications.ClearFromWindows;
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
                ClearFromWindows = ClearFromWindowsSwitch.IsChecked == true,
            },
            LaunchAtStartup = StartupSwitch.IsChecked == true,
        });
    }

    private void OnSizeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isShowingSettings || SizeBox.SelectedItem is not ComboBoxItem { Tag: IslandSize size })
        {
            return;
        }

        Change(_controller.Current with { Size = size });
    }

    private void OnLookChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isShowingSettings || LookBox.SelectedItem is not ComboBoxItem { Tag: IslandLook look })
        {
            return;
        }

        Change(_controller.Current with { Look = look });
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
