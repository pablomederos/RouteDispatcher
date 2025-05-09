using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.ConcreteServices;
using RouteDispatcher.Contracts;
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
            Assembly[] targetAssemblies = assemblies.Length == 0
                ? new[]
                    {
                        Assembly
                            .GetCallingAssembly()
                    }
                : assemblies;
                
            ConfigureServices(services, targetAssemblies);

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

            Dictionary<Type, CachedTypeConfiguration> cachedTypes = new();

            foreach (Type handlerType in requestHandlerTypes)
            {
                Type interfaceType = handlerType
                    .GetInterfaces()
                    .First(i => i.IsGenericType
                        && IsHandlerType(i)
                    );

                if (
                    configurationOptions is
                    {
                        UseHandlersCache: true, 
                        KeepCacheForEver: true
                    } 
                )
                {
                    Type requestType = interfaceType
                        .GetGenericArguments()
                        .First();

                    if(!cachedTypes.TryGetValue(requestType, out CachedTypeConfiguration? _))
                        cachedTypes.Add(requestType, new CachedTypeConfiguration
                        {
                            RequestType = requestType,
                            HandlerType = interfaceType,
                            CleanTimeout = configurationOptions.DiscardCachedHandlersTimeout,
                            KeepCacheForEver = configurationOptions.KeepCacheForEver 
                        });
                }
                services.AddTransient(interfaceType, handlerType);
            }

            if(configurationOptions.UseHandlersCache)
                services.AddSingleton<IHandlerCache, HandlerCacheService>(
                    BuildCachedTypes(cachedTypes)
                );
            
            services.AddScoped<IDispatcher, Dispatcher>(BuildDispatcher(configurationOptions));
            
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IMediator, Dispatcher>(BuildDispatcher(configurationOptions));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static Func<IServiceProvider, HandlerCacheService> BuildCachedTypes(Dictionary<Type, CachedTypeConfiguration> cachedTypes)
            => _ =>  new HandlerCacheService(cachedTypes); 

        private static Func<IServiceProvider, Dispatcher> BuildDispatcher(DispatcherConfiguration configurationOptions)
            => serviceProvider
            => new Dispatcher(serviceProvider, configurationOptions);

        private static Type[] GetHandlerTypes(this IEnumerable<Type> types)
            => types
                .Where(t => t.IsClass 
                    && t.GetInterfaces()
                        .Any(i => i.IsGenericType 
                              && IsHandlerType(i)
                        )
                )
                .ToArray();
        
        private static bool IsHandlerType(Type i)
            => i.GetGenericTypeDefinition() == Dispatcher.InvocationHandlerType
               || i.GetGenericTypeDefinition() == Dispatcher.MessageHandlerType;
    }
}