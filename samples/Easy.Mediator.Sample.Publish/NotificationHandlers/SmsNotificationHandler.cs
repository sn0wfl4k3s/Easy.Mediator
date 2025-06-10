namespace Easy.Mediator.Sample.Publish.NotificationHandlers;

public class SmsNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SMS] To: {notification.UserName} - {notification.Message}");

        return Task.CompletedTask;
    }
}
