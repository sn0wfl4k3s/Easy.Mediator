namespace Easy.Mediator.Sample.Send;

public record PingCommand(string Message) : IRequest<PongResponse>;
