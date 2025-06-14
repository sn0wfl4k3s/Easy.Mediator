using Microsoft.Extensions.DependencyInjection;
using System;
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

            foreach (var handler in handlerTypes)
            {
                switch (options.ServiceLifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(handler.InterfaceType, handler.HandlerType);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(handler.InterfaceType, handler.HandlerType);
                        break;
                    default:
                        services.AddTransient(handler.InterfaceType, handler.HandlerType);
                        break;
                }
            }
        }

        private static Assembly[] GetAssemblies(MediatorConfigurationOptions options)
        {
            if (options.AssemblyNames != null && options.AssemblyNames.Any())
            {
                return options.AssemblyNames
                    .Select(assemblyName => AppDomain.CurrentDomain.Load(assemblyName))
                    .ToArray();
            }

            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}

