using Microsoft.Extensions.DependencyInjection;
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

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var responseType = typeof(TResponse);

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _serviceProvider.GetRequiredService(handlerType);

            Func<Task<TResponse>> handlerDelegate = () =>
            {
                var method = handlerType.GetMethod("Handle");
                if (method == null)
                    throw new InvalidOperationException("Handler does not implement Handle method");

                return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            };

            var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var behaviors = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(behaviorInterfaceType))
                            ?? Enumerable.Empty<object>();

            foreach (var behavior in behaviors.Reverse())
            {
                var method = behaviorInterfaceType.GetMethod("Handle");

                var nextCopy = handlerDelegate;
                handlerDelegate = () =>
                {
                    return (Task<TResponse>)method!.Invoke(behavior, new object[] { request, cancellationToken, nextCopy })!;
                };
            }

            return await handlerDelegate();
        }



        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType));

            if (handlers == null || !handlers.Any())
                return;

            var tasks = handlers.Select(handler =>
            {
                var method = handlerType.GetMethod("Handle");
                return (Task)method!.Invoke(handler, new object[] { notification, cancellationToken })!;
            });

            await Task.WhenAll(tasks);
        }
    }
}
