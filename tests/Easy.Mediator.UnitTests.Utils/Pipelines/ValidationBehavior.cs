using Easy.Mediator.UnitTests.Utils.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Pipelines;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        if (request is UserCreateCommand cmd && string.IsNullOrWhiteSpace(cmd.Name))
        {
            throw new ArgumentException("Name is required");
        }

        return await next();
    }
}
