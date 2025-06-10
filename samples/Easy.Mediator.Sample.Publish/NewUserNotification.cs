namespace Easy.Mediator.Sample.Publish;

public record NewUserNotification(string UserName, string Message) : INotification;
