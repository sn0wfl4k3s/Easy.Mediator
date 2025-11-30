namespace Easy.Mediator.Sample.Send;

public class PingCommand : IRequest<PongResponse>
{
    public string Message { get; }

    public PingCommand(string message)
    {
        Message = message;
    }
}
