namespace Easy.Mediator.Sample.Send;

public class PongResponse
{
    public string Message { get; }

    public PongResponse(string message)
    {
        Message = message;
    }
}