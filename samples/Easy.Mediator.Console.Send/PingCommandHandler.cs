namespace Easy.Mediator.Console.Send;

public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
    {
        var response = new PongResponse($"{request.Message} => Pong!");

        return Task.FromResult(response);
    }
}
