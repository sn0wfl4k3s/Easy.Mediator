namespace Easy.Mediator.Console.Send;

public record PingCommand(string Message) : IRequest<PongResponse>;
