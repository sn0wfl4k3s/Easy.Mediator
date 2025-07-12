using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Easy.Mediator
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

    public interface IPipelineBehavior<TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
