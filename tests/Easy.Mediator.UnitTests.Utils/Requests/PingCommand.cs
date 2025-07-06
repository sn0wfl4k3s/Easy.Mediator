namespace Easy.Mediator.UnitTests.Utils.Requests;

public record PingCommand(string Message) : IRequest<PongResponse>;
