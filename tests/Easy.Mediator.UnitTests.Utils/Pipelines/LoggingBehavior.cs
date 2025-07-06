using System;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Pipelines;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public static bool Executed = false;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        Executed = true;
        return await next();
    }
}