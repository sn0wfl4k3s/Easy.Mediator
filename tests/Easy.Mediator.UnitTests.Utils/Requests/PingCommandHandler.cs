using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Requests;

public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PongResponse($"{request.Message} => Pong!"));
}