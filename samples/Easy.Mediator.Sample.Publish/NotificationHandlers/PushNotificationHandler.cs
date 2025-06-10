namespace Easy.Mediator.Sample.Publish.NotificationHandlers;

public class PushNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Push] To: {notification.UserName} - {notification.Message}");

        return Task.CompletedTask;
    }
}
