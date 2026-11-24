using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Extension methods for registering mediator services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the mediator and all handlers from the specified assembly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly to scan for handlers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return AddMediatorCore(services, null, LoadableTypes(assembly));
        }

        /// <summary>
        /// Registers the mediator and all handlers from multiple assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies to scan for handlers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
            => AddMediator(services, null!, assemblies);

        /// <summary>
        /// Registers the mediator with options and all handlers from the given assemblies. Registration is
        /// idempotent: repeated calls register each service once and keep the options of the first call.
        /// Two different handlers for the same request and result type are an error. Scanned handlers are scoped;
        /// a handler registered before this call keeps its own registration and lifetime.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Configures <see cref="MediatorOptions"/> (telemetry).</param>
        /// <param name="assemblies">The assemblies to scan for handlers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> configure, params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            return AddMediatorCore(services, configure, assemblies.Where(a => a != null).SelectMany(LoadableTypes));
        }

        internal static IServiceCollection AddMediatorCore(IServiceCollection services, Action<MediatorOptions>? configure, IEnumerable<Type> candidateTypes)
        {
            var options = services.FirstOrDefault(d => d.ServiceType == typeof(MediatorOptions))?.ImplementationInstance as MediatorOptions;
            if (options == null)
            {
                options = new MediatorOptions();
                configure?.Invoke(options);
                services.TryAddSingleton(options);
            }

            services.TryAdd(ServiceDescriptor.Scoped<IMediator>(sp => new Mediator(sp, sp.GetRequiredService<MediatorOptions>())));

            foreach (var type in candidateTypes)
            {
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                    continue;

                foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>)))
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == contract);
                    if (existing == null)
                    {
                        services.Add(new ServiceDescriptor(contract, type, ServiceLifetime.Scoped));
                        continue;
                    }

                    if (existing.ImplementationType == null || existing.ImplementationType == type)
                        continue; // already registered (same handler, or a user factory/instance)

                    var args = contract.GetGenericArguments();
                    throw new InvalidOperationException(
                        $"Two handlers for request {Dispatch.TypeNames.Get(args[0])} returning {Dispatch.TypeNames.Get(args[1])}: {existing.ImplementationType.FullName} and {type.FullName}. Register only one.");
                }
            }

            return services;
        }

        internal static IEnumerable<Type> LoadableTypes(Assembly assembly)
            => LoadableTypes(() => assembly.DefinedTypes);

        internal static IEnumerable<Type> LoadableTypes(Func<IEnumerable<Type>> load)
        {
            try
            {
                return load().ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Cast<Type>().ToList();
            }
        }
    }
}
