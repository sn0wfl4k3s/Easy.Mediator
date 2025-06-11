namespace Easy.Mediator.UnitTests;

public record PingCommand(string Message) : IRequest<PongResponse>;