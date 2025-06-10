using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator
{
    public interface IMediator
    {
        Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
    }
}
