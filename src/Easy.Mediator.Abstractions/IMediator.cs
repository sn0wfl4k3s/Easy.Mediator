using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Easy.Mediator
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public interface IMediator
    {
        Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
    }
}
