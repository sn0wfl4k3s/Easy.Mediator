namespace Easy.Mediator.UnitTests.Utils.Requests;

public class PingCommand : IRequest<PongResponse>
{
    public string Message { get; }

    public PingCommand(string message)
    {
        Message = message;
    }
}
