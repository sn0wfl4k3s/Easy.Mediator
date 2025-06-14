using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;


namespace Easy.Mediator
{
    public class MediatorConfigurationOptions
    {
        public List<string> AssemblyNames { get; private set; }
        public List<Assembly> Assemblies { get; private set; }
        public ServiceLifetime ServiceLifetime { get; private set; }

        public MediatorConfigurationOptions()
        {
            AssemblyNames = new List<string>();
            Assemblies = new List<Assembly>();
            ServiceLifetime = ServiceLifetime.Transient;
        }

        public MediatorConfigurationOptions AddAssembliesFrom(params string[] assembliesName)
        {
            AssemblyNames.AddRange(assembliesName);

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
    }
}

