#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using RouteDispatcher.Contracts;
using RouteDispatcher.ConcreteServices;
using System;
using System.Collections.Generic;
using RouteDispatcher.Models;

namespace RouteDispatcher.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRouteDispatcher(this IServiceCollection services, Action<DispatcherConfiguration> options)
        {
            if(options == null)
                throw new ArgumentNullException(nameof(options), "Configuration action cannot be null.");

            var configurationOptions = new DispatcherConfiguration();
            options(configurationOptions);



            configurationOptions.Assemblies = configurationOptions
                .Assemblies
                .Length == 0
                ? new[]
                    {
                        Assembly
                            .GetCallingAssembly()
                    }
                : configurationOptions
                    .Assemblies;

            ConfigureServices(services, configurationOptions);

            return services;
        }

        public static IServiceCollection AddRouteDispatcher(this IServiceCollection services, params Assembly[] assemblies)
        {
            Assembly[] targetAssembies = assemblies.Length == 0
                ? new[]
                    {
                        Assembly
                            .GetCallingAssembly()
                    }
                : assemblies;
                
            ConfigureServices(services, targetAssembies);

            return services;
        }

        private static void ConfigureServices(
            IServiceCollection services,
            Assembly[] assemblies
        ) => ConfigureServices(services, new DispatcherConfiguration()
        { 
            Assemblies = assemblies
        });

        private static void ConfigureServices(
            IServiceCollection services,
            DispatcherConfiguration configurationOptions
        )
        {
            Type[] requestHandlerTypes = configurationOptions
                    .Assemblies
                    .SelectMany(a => a.GetTypes())
                    .GetHandlerTypes();

            foreach (var handlerType in requestHandlerTypes)
            {
                var interfaceType = handlerType
                    .GetInterfaces()
                    .First(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                    );
                services.AddTransient(interfaceType, handlerType);
            }

            services.AddSingleton<IHandlerCache, HandlerCacheService>();
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IMediator, Dispatcher>(BuildDispatcher(configurationOptions));
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddScoped<IDispatcher, Dispatcher>(BuildDispatcher(configurationOptions));
        }

        private static Func<IServiceProvider, Dispatcher> BuildDispatcher(DispatcherConfiguration configurationOptions)
            => serviceProvider
            => new Dispatcher(serviceProvider, configurationOptions);

        private static Type[] GetHandlerTypes(this IEnumerable<Type> types)
            => types
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToArray();
    }
}