namespace Easy.Mediator.UnitTests.Requests;

public record PingCommand(string Message) : IRequest<PongResponse>;