// NotificationStackTests.cs: checks stacking per chat on the notifications page (PRD FR-35,
// AC-26): messages from the same chat share one stack with a count, each chat keeps the text of
// its last 3 messages only, the page holds 3 chats, and a chat read in the app leaves whole.

using WinIsland.Core.Notifications;

namespace WinIsland.Core.Tests.Notifications;

public class NotificationStackTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 20, 0, 0, TimeSpan.FromHours(7));
    private static readonly TimeSpan Six = TimeSpan.FromSeconds(6);

    [Fact]
    public void Three_from_one_chat_and_one_from_another_give_two_stacks()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam", "one"), Now, Six);
        feed.Add(Message(2, "Sam", "two"), Now, Six);
        feed.Add(Message(3, "Sam", "three"), Now, Six);
        feed.Add(Message(4, "Kai", "hi"), Now, Six);

        Assert.Collection(
            feed.Stacks,
            kai => Assert.Equal((4u, 1), (kai.Newest.WindowsId, kai.Count)),
            sam =>
            {
                Assert.Equal("three", sam.Newest.Preview);
                Assert.Equal(3, sam.Count);
                Assert.Equal("3 messages", sam.CountText);
            });
    }

    [Fact]
    public void A_chat_keeps_the_text_of_its_last_3_messages_and_counts_all_of_them()
    {
        NotificationFeed feed = new();
        for (uint id = 1; id <= 5; id++)
        {
            feed.Add(Message(id, "Sam", $"message {id}"), Now, Six);
        }

        NotificationStack sam = Assert.Single(feed.Stacks);
        Assert.Equal(5, sam.Count);
        Assert.Equal(new[] { "message 5", "message 4", "message 3" }, sam.Messages.Select(message => message.Preview));
        Assert.Equal(3, feed.Recent.Count);
    }

    [Fact]
    public void The_page_holds_3_chats_and_a_returning_chat_counts_from_one_again()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Sam"), Now, Six);
        feed.Add(Message(3, "Kai"), Now, Six);
        feed.Add(Message(4, "Ana"), Now, Six);
        feed.Add(Message(5, "Rio"), Now, Six);

        Assert.Equal(new[] { "Rio", "Ana", "Kai" }, feed.Stacks.Select(stack => stack.Newest.Sender));

        feed.Add(Message(6, "Sam"), Now, Six);
        Assert.Equal((6u, 1), (feed.Stacks[0].Newest.WindowsId, feed.Stacks[0].Count));
    }

    [Fact]
    public void A_new_message_moves_its_chat_to_the_top()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam", "first"), Now, Six);
        feed.Add(Message(2, "Kai"), Now, Six);
        feed.Add(Message(3, "Sam", "second"), Now, Six);

        Assert.Equal(new[] { "Sam", "Kai" }, feed.Stacks.Select(stack => stack.Newest.Sender));
        Assert.Equal(2, feed.Stacks[0].Count);
    }

    [Fact]
    public void Reading_a_chat_in_the_app_removes_its_whole_stack()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam"), Now, Six);
        feed.Add(Message(2, "Sam"), Now, Six);
        feed.Add(Message(3, "Kai"), Now, Six);

        feed.MarkRemoved(2, Now);
        feed.UpdateTime(Now.AddSeconds(1));

        Assert.Equal("Kai", Assert.Single(feed.Stacks).Newest.Sender);
    }

    [Fact]
    public void A_message_posted_twice_does_not_raise_the_count()
    {
        NotificationFeed feed = new();
        feed.Add(Message(1, "Sam", "see you"), Now, Six);
        feed.Add(Message(2, "Sam", "see you"), Now, Six);

        Assert.Equal(1, Assert.Single(feed.Stacks).Count);
    }

    [Theory]
    [InlineData(1, "", false)]
    [InlineData(2, "2 messages", true)]
    [InlineData(99, "99 messages", true)]
    [InlineData(100, "99+ messages", true)]
    public void The_count_text_and_opening_follow_the_count(int count, string text, bool canOpen)
    {
        NotificationStack stack = new([Message(1, "Sam")], count);

        Assert.Equal(text, stack.CountText);
        Assert.Equal(canOpen, stack.CanOpen);
    }

    private static IslandNotification Message(uint id, string sender, string text = "hello")
    {
        return new IslandNotification(id, NotificationSource.WhatsApp, "WhatsApp", sender, text, Now);
    }
}
