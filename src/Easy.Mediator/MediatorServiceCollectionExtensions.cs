using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyMediator(this IServiceCollection services, Action<MediatorConfigurationOptions>? configureOptions = null)
        {
            var config = new MediatorConfigurationOptions();

            configureOptions?.Invoke(config);

            // Register Mediator as singleton
            services.AddSingleton<IMediator>(provider => new Mediator(provider));

            // Register handlers and behaviors with configured lifetime
            RegisterHandlersFromAssemblies(services, config);
            RegisterPipelines(services, config);

            return services;
        }

        private static void RegisterPipelines(IServiceCollection services, MediatorConfigurationOptions config)
        {
            foreach (var behaviorType in config.PipelineBehaviors)
            {
                if (!behaviorType.IsGenericTypeDefinition)
                {
                    // Non-generic behavior
                    var implementedInterfaces = behaviorType.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                        .ToList();

                    foreach (var interfaceType in implementedInterfaces)
                    {
                        // Register behavior in DI container
                        switch (config.ServiceLifetime)
                        {
                            case ServiceLifetime.Transient:
                                services.AddTransient(interfaceType, behaviorType);
                                break;
                            case ServiceLifetime.Scoped:
                                services.AddScoped(interfaceType, behaviorType);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(interfaceType, behaviorType);
                                break;
                        }

                        var genericArgs = interfaceType.GetGenericArguments();
                        var requestType = genericArgs[0];
                        var responseType = genericArgs[1];

                        var method = typeof(Mediator).GetMethod(nameof(Mediator.RegisterPipelineBehavior));
                        var genericMethod = method?.MakeGenericMethod(requestType, responseType);
                        genericMethod?.Invoke(null, new object[] { behaviorType });
                    }
                }
                else
                {
                    // Generic behavior - register for all matching request types
                    var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
                    var assemblies = GetAssemblies(config);

                    var requestTypes = assemblies
                        .SelectMany(x => x.GetTypes())
                        .Where(t => !t.IsAbstract && !t.IsInterface)
                        .SelectMany(t => t.GetInterfaces()
                            .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == requestHandlerInterfaceType)
                            .Select(i => new
                            {
                                RequestType = i.GetGenericArguments()[0],
                                ResponseType = i.GetGenericArguments()[1]
                            }))
                        .ToList();

                    // Remove duplicates
                    var uniqueRequestTypes = new List<(Type, Type)>();
                    foreach (var item in requestTypes)
                    {
                        var tuple = (item.RequestType, item.ResponseType);
                        if (!uniqueRequestTypes.Any(x => x.Item1 == tuple.Item1 && x.Item2 == tuple.Item2))
                        {
                            uniqueRequestTypes.Add(tuple);
                        }
                    }

                    foreach (var (requestType, responseType) in uniqueRequestTypes)
                    {
                        var genericBehaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
                        var concreteBehaviorType = behaviorType.MakeGenericType(requestType, responseType);

                        // Register concrete generic behavior in DI container
                        switch (config.ServiceLifetime)
                        {
                            case ServiceLifetime.Transient:
                                services.AddTransient(genericBehaviorInterfaceType, concreteBehaviorType);
                                break;
                            case ServiceLifetime.Scoped:
                                services.AddScoped(genericBehaviorInterfaceType, concreteBehaviorType);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(genericBehaviorInterfaceType, concreteBehaviorType);
                                break;
                        }

                        var method = typeof(Mediator).GetMethod(nameof(Mediator.RegisterPipelineBehavior));
                        var genericMethod = method?.MakeGenericMethod(requestType, responseType);
                        genericMethod?.Invoke(null, new object[] { behaviorType });
                    }
                }
            }
        }

        private static void RegisterHandlersFromAssemblies(IServiceCollection services, MediatorConfigurationOptions options)
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

            foreach (var item in handlerTypes)
            {
                var interfaceType = item.InterfaceType;

                // Register handler in DI container with configured lifetime
                switch (options.ServiceLifetime)
                {
                    case ServiceLifetime.Transient:
                        services.AddTransient(interfaceType, item.HandlerType);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(interfaceType, item.HandlerType);
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(interfaceType, item.HandlerType);
                        break;
                }

                if (interfaceType.GetGenericTypeDefinition() == requestHandlerInterfaceType)
                {
                    var genericArgs = interfaceType.GetGenericArguments();
                    var requestType = genericArgs[0];
                    var responseType = genericArgs[1];

                    // Register only the type, not an instance
                    var method = typeof(Mediator).GetMethod(nameof(Mediator.RegisterRequestHandler));
                    var genericMethod = method?.MakeGenericMethod(requestType, responseType);
                    
                    // Create a factory function that will resolve from DI later
                    var handlerFactory = (object)Activator.CreateInstance(item.HandlerType)!;
                    genericMethod?.Invoke(null, new object[] { handlerFactory });
                }
                else if (interfaceType.GetGenericTypeDefinition() == notificationHandlerInterfaceType)
                {
                    var genericArgs = interfaceType.GetGenericArguments();
                    var notificationType = genericArgs[0];

                    // Register only the type, not an instance
                    var method = typeof(Mediator).GetMethod(nameof(Mediator.RegisterNotificationHandler));
                    var genericMethod = method?.MakeGenericMethod(notificationType);
                    
                    // Create a factory function that will resolve from DI later
                    var handlerFactory = (object)Activator.CreateInstance(item.HandlerType)!;
                    genericMethod?.Invoke(null, new object[] { handlerFactory });
                }
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


