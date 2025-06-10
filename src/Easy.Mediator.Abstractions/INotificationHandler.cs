using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator
{
    public interface INotificationHandler<TNotification> where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
