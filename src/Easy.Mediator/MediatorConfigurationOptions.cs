using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Easy.Mediator
{
    public class MediatorConfigurationOptions
    {
        internal List<Assembly> Assemblies { get; private set; }
        internal List<Type> PipelineBehaviors { get; private set; }
        internal ServiceLifetime ServiceLifetime { get; private set; }

        public MediatorConfigurationOptions()
        {
            Assemblies = new List<Assembly>();
            ServiceLifetime = ServiceLifetime.Transient;
            PipelineBehaviors = new List<Type>();
        }

        public MediatorConfigurationOptions AddAssembliesFrom(params string[] assembliesName)
        {
            var assemblies = assembliesName
                .Select(assemblyName => AppDomain.CurrentDomain.Load(assemblyName))
                .ToList();

            Assemblies.AddRange(assemblies);

            return this;
        }

        public MediatorConfigurationOptions AddAssembliesFrom(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);

            return this;
        }

        public MediatorConfigurationOptions SetServiceLifetime(ServiceLifetime serviceLifetime)
        {
            ServiceLifetime = serviceLifetime;

            return this;
        }

        public MediatorConfigurationOptions UseScopedServiceLifetime()
        {
            ServiceLifetime = ServiceLifetime.Scoped;

            return this;
        }

        public MediatorConfigurationOptions UseSingletonServiceLifetime()
        {
            ServiceLifetime = ServiceLifetime.Singleton;

            return this;
        }

        public MediatorConfigurationOptions UseTransientServiceLifetime()
        {
            ServiceLifetime = ServiceLifetime.Transient;

            return this;
        }

        public MediatorConfigurationOptions AddPipelineBehavior(Type openGenericType)
        {
            PipelineBehaviors.Add(openGenericType);

            return this;
        }
    }
}

