namespace Easy.Mediator.UnitTests.Utils.Requests;

public class PongResponse
{
    public string Message { get; }

    public PongResponse(string message)
    {
        Message = message;
    }
}