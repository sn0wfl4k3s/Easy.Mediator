namespace Easy.Mediator.UnitTests.Requests;

public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PongResponse($"{request.Message} => Pong!"));
}