using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator
{
    public class Mediator : IMediator
    {
        private static readonly List<(Type, object)> RequestHandlerRegistry = new List<(Type, object)>();
        private static readonly List<(Type, object)> NotificationHandlerRegistry = new List<(Type, object)>();
        private static readonly HashSet<(Type, Type)> PipelineBehaviorRegistry = new HashSet<(Type, Type)>();
        private static readonly Channel<object> RequestAuditChannel = Channel.CreateUnbounded<object>();
        private static readonly Channel<object> NotificationAuditChannel = Channel.CreateUnbounded<object>();
        private static readonly object RegistryLock = new object();

        private readonly IServiceProvider? _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void RegisterRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse>
        {
            var key = typeof(TRequest);
            var message = (key, (object)handler);
            lock (RegistryLock)
            {
                RequestHandlerRegistry.Add(message);
            }
        }

        public static void RegisterNotificationHandler<TNotification>(INotificationHandler<TNotification> handler)
            where TNotification : INotification
        {
            var key = typeof(TNotification);
            var message = (key, (object)handler);
            lock (RegistryLock)
            {
                NotificationHandlerRegistry.Add(message);
            }
        }

        public static void RegisterPipelineBehavior<TRequest, TResponse>(Type behaviorType)
            where TRequest : IRequest<TResponse>
        {
            var key = typeof(TRequest);
            var behaviorKey = (key, behaviorType);
            
            lock (RegistryLock)
            {
                // Only add if not already present (avoid duplicates)
                PipelineBehaviorRegistry.Add(behaviorKey);
            }
        }

        public static void ClearPipelineBehaviors()
        {
            lock (RegistryLock)
            {
                PipelineBehaviorRegistry.Clear();
            }
        }

        public static void ClearHandlers()
        {
            lock (RegistryLock)
            {
                RequestHandlerRegistry.Clear();
                NotificationHandlerRegistry.Clear();
            }
        }

        private static object GetRequestHandler(Type requestType)
        {
            lock (RegistryLock)
            {
                var handler = RequestHandlerRegistry.LastOrDefault(r => r.Item1 == requestType);
                
                if (handler.Item2 == null)
                    throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");

                return handler.Item2;
            }
        }

        private static List<object> GetNotificationHandlers(Type notificationType)
        {
            lock (RegistryLock)
            {
                var handlers = new List<object>();
                foreach (var (type, handler) in NotificationHandlerRegistry)
                {
                    if (type == notificationType)
                    {
                        handlers.Add(handler);
                    }
                }
                return handlers;
            }
        }

        private static List<Type> GetPipelineBehaviors(Type requestType)
        {
            var behaviors = new List<Type>();
            
            lock (RegistryLock)
            {
                foreach (var (type, behaviorType) in PipelineBehaviorRegistry)
                {
                    if (type == requestType)
                    {
                        behaviors.Add(behaviorType);
                    }
                }
            }

            return behaviors;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var handlerObj = GetRequestHandler(requestType);
            var behaviors = GetPipelineBehaviors(requestType);

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            RequestHandlerDelegate<TResponse> handlerDelegate = (ct) =>
            {
                var method = handlerType.GetMethod("Handle");
                if (method == null)
                    throw new InvalidOperationException("Handler does not implement Handle method");

                return (Task<TResponse>)method.Invoke(handlerObj, new object[] { request, ct })!;
            };

            // Apply pipeline behaviors in reverse order
            if (behaviors != null && behaviors.Count > 0)
            {
                var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
                for (int i = behaviors.Count - 1; i >= 0; i--)
                {
                    var behaviorType = behaviors[i];
                    var concreteType = behaviorType.IsGenericTypeDefinition
                        ? behaviorType.MakeGenericType(requestType, typeof(TResponse))
                        : behaviorType;

                    object behavior;
                    try
                    {
                        // Try to resolve from DI container first
                        if (_serviceProvider != null)
                        {
                            try
                            {
                                // Resolve by the interface type, not the concrete type
                                behavior = _serviceProvider.GetRequiredService(behaviorInterfaceType);
                            }
                            catch
                            {
                                // Fall back to Activator if DI fails
                                behavior = Activator.CreateInstance(concreteType);
                            }
                        }
                        else
                        {
                            behavior = Activator.CreateInstance(concreteType);
                        }
                    }
                    catch (MissingMethodException)
                    {
                        continue;
                    }

                    if (behavior != null)
                    {
                        var nextCopy = handlerDelegate;
                        var behaviorMethod = behaviorInterfaceType.GetMethod("Handle");

                        handlerDelegate = (ct) =>
                        {
                            return (Task<TResponse>)behaviorMethod!.Invoke(behavior, new object[] { request, nextCopy, ct })!;
                        };
                    }
                }
            }

            // Write request to audit channel
            await RequestAuditChannel.Writer.WriteAsync(request, cancellationToken);

            return await handlerDelegate(cancellationToken);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            var key = typeof(TNotification);
            var handlers = GetNotificationHandlers(key);

            if (handlers.Count == 0)
                return;

            var handlerType = typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification));
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("Handle");
                var task = (Task)method!.Invoke(handler, new object[] { notification, cancellationToken })!;
                tasks.Add(task);
            }

            // Write notification to audit channel
            await NotificationAuditChannel.Writer.WriteAsync(notification, cancellationToken);

            if (tasks.Count > 0)
                await Task.WhenAll(tasks);
        }

        // Method to read audit trail from request channel
        public static async IAsyncEnumerable<object> ReadRequestAuditAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var request in RequestAuditChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return request;
            }
        }

        // Method to read audit trail from notification channel
        public static async IAsyncEnumerable<object> ReadNotificationAuditAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var notification in NotificationAuditChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return notification;
            }
        }
    }
}

