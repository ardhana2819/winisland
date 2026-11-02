// NotificationsPageView.xaml.cs: fills the notifications page (PRD FR-22, FR-26, FR-35).
// The list shows one stacked card per chat (StackCard.cs). A stack of 2 or more is clickable and
// opens the chat (ChatView.cs); clicking the open chat goes back. Both clicks are reported to the
// island's rules, which decide; ChatSlide.cs moves between the two views.
// "Hide message text" is applied through NotificationSettings (FR-17).
// Other clicks on a card are swallowed, so they neither open an app nor close the island (FR-22).
// The page is rebuilt only when what it shows changes, so a running move is not cut off.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

public partial class NotificationsPageView : UserControl
{
    private const double CardGapDips = 6;
    private const double SingleTopDips = 10;

    private readonly ChatSlide _slide;
    private string _shownContent = string.Empty;
    private bool _showsChat;

    public NotificationsPageView()
    {
        InitializeComponent();
        _slide = new ChatSlide(Cards);
    }

    /// <summary>A stack of 2 or more was clicked; carries its newest message.</summary>
    public event EventHandler<IslandNotification>? StackClicked;

    /// <summary>The open chat was clicked: back to the list.</summary>
    public event EventHandler? ChatClicked;

    public void Show(
        IReadOnlyList<NotificationStack> stacks,
        NotificationStack? openChat,
        NotificationSettings settings,
        Func<IslandNotification, ImageSource?> logoFor)
    {
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logoFor);

        string content = Describe(stacks, openChat, settings, logoFor);
        if (content == _shownContent)
        {
            return;
        }

        _shownContent = content;
        void Fill() => FillPage(stacks, openChat, settings, logoFor);

        bool showChat = openChat is not null;
        if (showChat != _showsChat)
        {
            _showsChat = showChat;
            _slide.Run(opening: showChat, Fill);
        }
        else
        {
            _slide.Refill(Fill);
        }
    }

    private void FillPage(IReadOnlyList<NotificationStack> stacks, NotificationStack? openChat, NotificationSettings settings, Func<IslandNotification, ImageSource?> logoFor)
    {
        Cards.Children.Clear();
        if (openChat is not null)
        {
            FillChat(openChat, settings, logoFor);
            return;
        }

        EmptyText.Visibility = stacks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        bool isSingle = stacks.Count == 1;
        Cards.VerticalAlignment = isSingle ? VerticalAlignment.Top : VerticalAlignment.Center;
        Cards.Margin = new Thickness(10, isSingle ? SingleTopDips : 0, 10, 0);

        for (int index = 0; index < stacks.Count; index++)
        {
            NotificationStack stack = stacks[index];
            IslandNotification newest = stack.Newest;
            FrameworkElement card = NotificationCard.Build(newest, settings.PreviewFor(newest), logoFor(newest), isSingle, stack.CountText);
            FrameworkElement shown = StackCard.Wrap(card, stack.Count);
            shown.Margin = new Thickness(0, index == 0 ? 0 : CardGapDips, 0, 0);
            if (stack.CanOpen)
            {
                shown.Cursor = Cursors.Hand;
                shown.MouseLeftButtonUp += (_, e) =>
                {
                    e.Handled = true;
                    StackClicked?.Invoke(this, newest);
                };
            }
            else
            {
                shown.MouseLeftButtonUp += SwallowClick;
            }

            Cards.Children.Add(shown);
        }
    }

    private void FillChat(NotificationStack chat, NotificationSettings settings, Func<IslandNotification, ImageSource?> logoFor)
    {
        EmptyText.Visibility = Visibility.Collapsed;
        Cards.VerticalAlignment = VerticalAlignment.Center;
        Cards.Margin = new Thickness(10, 0, 10, 0);
        foreach (FrameworkElement part in ChatView.Build(chat, settings, logoFor(chat.Newest)))
        {
            part.Cursor = Cursors.Hand;
            part.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                ChatClicked?.Invoke(this, EventArgs.Empty);
            };
            Cards.Children.Add(part);
        }
    }

    // Everything the page shows, as text, to tell whether it changed. Ids and counts only; the
    // message text is compared through the ids, so it is never copied anywhere (FR-18).
    private static string Describe(IReadOnlyList<NotificationStack> stacks, NotificationStack? openChat, NotificationSettings settings, Func<IslandNotification, ImageSource?> logoFor)
    {
        IEnumerable<string> chats = stacks.Select(stack =>
            $"{string.Join(",", stack.Messages.Select(message => message.WindowsId))}x{stack.Count}{(logoFor(stack.Newest) is null ? string.Empty : "L")}");
        return $"{string.Join(";", chats)}|open:{openChat?.Newest.WindowsId}|hide:{settings.HideMessageText}";
    }

    private static void SwallowClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
