namespace Easy.Mediator.Sample.Publish;

public class NewUserNotification : INotification
{
    public string UserName { get; }
    public string Message { get; }

    public NewUserNotification(string userName, string message)
    {
        UserName = userName;
        Message = message;
    }
}
