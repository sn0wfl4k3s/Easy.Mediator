using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator
{
    public interface IRequestHandler<TRequest, TResult> where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
