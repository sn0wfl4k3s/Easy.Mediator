using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator
{
    internal class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResult));

            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"Handler for '{request.GetType().Name}' not found.");

            return (Task<TResult>)handlerType.GetMethod("Handle").Invoke(handler, new object[] { request, cancellationToken });
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());

            var handlers = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType));

            if (handlers == null || !handlers.Any())
                return;

            var tasks = handlers.Select(handler => (Task)handlerType.GetMethod("Handle").Invoke(handler, new object[] { notification, cancellationToken }));

            await Task.WhenAll(tasks);
        }
    }
}
