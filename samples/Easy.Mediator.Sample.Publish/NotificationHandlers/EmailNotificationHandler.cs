namespace Easy.Mediator.Sample.Publish.NotificationHandlers;

public class EmailNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Email] To: {notification.UserName} - {notification.Message}");

        return Task.CompletedTask;
    }
}
