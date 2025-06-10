using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Easy.Mediator
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyMediator(this IServiceCollection services)
        {
            services.AddSingleton<IMediator, Mediator>();

            services.RegisterHandlersFromAssemblies();

            return services;
        }

        private static void RegisterHandlersFromAssemblies(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var handlerInterfaceType = typeof(IRequestHandler<,>);

            var handlerTypes = assemblies.SelectMany(x => x.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .Select(i => new { HandlerType = t, InterfaceType = i }))
                .ToList());

            foreach (var handler in handlerTypes)
            {
                services.AddTransient(handler.InterfaceType, handler.HandlerType);
            }
        }
    }
}
