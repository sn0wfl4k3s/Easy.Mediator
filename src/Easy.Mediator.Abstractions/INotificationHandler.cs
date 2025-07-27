using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Easy.Mediator
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public interface INotificationHandler<TNotification> where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
