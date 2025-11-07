using Microsoft.Extensions.DependencyInjection;
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

            services.AddScoped<IMediator, Mediator>();

            var handlerTypes = FindTypes(assembly)
                .SelectMany(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>))
                    .Select(i => new { Interface = i, Implementation = type }))
                .ToList();

            foreach (var handler in handlerTypes)
                services.AddScoped(handler.Interface, handler.Implementation);

            return services;
        }

        /// <summary>
        /// Registers the mediator and all handlers from multiple assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies to scan for handlers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            services.AddScoped<IMediator, Mediator>();

            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                    continue;

                var handlerTypes = FindTypes(assembly)
                    .SelectMany(type => type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>))
                        .Select(i => new { Interface = i, Implementation = type }))
                    .ToList();

                foreach (var handler in handlerTypes)
                    services.AddScoped(handler.Interface, handler.Implementation);
            }

            return services;
        }

        private static IEnumerable<Type> FindTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface);
        }
    }
}