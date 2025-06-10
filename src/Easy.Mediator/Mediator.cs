using System;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator
{
    public class Mediator : IMediator
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
    }
}
