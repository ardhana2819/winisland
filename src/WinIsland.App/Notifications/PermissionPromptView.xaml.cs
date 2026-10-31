// PermissionPromptView.xaml.cs: reports a click on Allow. The window opens Windows Settings.

using System.Windows;
using System.Windows.Controls;

namespace WinIsland.App.Notifications;

public partial class PermissionPromptView : UserControl
{
    public PermissionPromptView()
    {
        InitializeComponent();
    }

    public event EventHandler? AllowClicked;

    private void OnAllowClick(object sender, RoutedEventArgs e) => AllowClicked?.Invoke(this, EventArgs.Empty);
}
