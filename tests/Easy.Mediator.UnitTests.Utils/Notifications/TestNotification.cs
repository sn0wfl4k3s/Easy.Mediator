namespace Easy.Mediator.UnitTests.Utils.Notifications;

public class TestNotification : INotification
{
    public string Content { get; }

    public TestNotification(string content)
    {
        Content = content;
    }
}