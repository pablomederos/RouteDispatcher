using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using RouteDispatcher.Contracts;
using RouteDispatcher.ConcreteServices;
using System;
using System.Collections.Generic;

namespace RouteDispatcher.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRouteDispatcher(this IServiceCollection services, params Assembly[] assemblies)
        {
            var requestHandlerTypes = assemblies.Length == 0
                ? Assembly
                    .GetCallingAssembly()
                    .GetTypes()
                    .GetHandlerTypes()
                : assemblies
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
            services.AddScoped<IMediator, Mediator>();

            return services;
        }

        private static Type[] GetHandlerTypes(this IEnumerable<Type> types)
            => types
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToArray();
    }
}