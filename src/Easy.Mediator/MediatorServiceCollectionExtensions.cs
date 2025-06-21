using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Easy.Mediator
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyMediator(this IServiceCollection services, Action<MediatorConfigurationOptions>? configureOptions = null)
        {
            var config = new MediatorConfigurationOptions();

            configureOptions?.Invoke(config);

            services.AddSingleton<IMediator, Mediator>();

            services.RegisterHandlersFromAssemblies(config);

            return services;
        }

        private static void RegisterHandlersFromAssemblies(this IServiceCollection services, MediatorConfigurationOptions options)
        {
            var assemblies = GetAssemblies(options);

            var requestHandlerInterfaceType = typeof(IRequestHandler<,>);

            var notificationHandlerInterfaceType = typeof(INotificationHandler<>);

            var handlerTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        (i.GetGenericTypeDefinition() == requestHandlerInterfaceType ||
                         i.GetGenericTypeDefinition() == notificationHandlerInterfaceType))
                    .Select(i => new { HandlerType = t, InterfaceType = i }))
                .ToList();

            switch (options.ServiceLifetime)
            {
                case ServiceLifetime.Transient:
                    foreach (var handler in handlerTypes)
                        services.AddTransient(handler.InterfaceType, handler.HandlerType);
                    break;
                case ServiceLifetime.Scoped:
                    foreach (var handler in handlerTypes)
                        services.AddScoped(handler.InterfaceType, handler.HandlerType);
                    break;
                default:
                    foreach (var handler in handlerTypes)
                        services.AddSingleton(handler.InterfaceType, handler.HandlerType);
                    break;
            }
        }

        private static IEnumerable<Assembly> GetAssemblies(MediatorConfigurationOptions options)
        {
            if (options.Assemblies != null && options.Assemblies.Any())
            {
                return options.Assemblies;
            }

            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}

