using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Models;

namespace RouteDispatcher.ConcreteServices;

public sealed partial class Dispatcher
{
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
        => Send<Empty>(request, cancellationToken);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Type requestType = request.GetType();

        if (!_configurationOptions.UseHandlersCache)
            return GetHandler<TResponse>(requestType)
                (request, cancellationToken);
        
        CachedHandlerItem cached = GetCachedHandler<TResponse>(requestType);
            
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        if (!keepCacheForEver)
            cached.Refresh(cleanupTimeout);

        object handler = _serviceProvider
                             .GetService(cached.HandlerType)
                         ?? throw new HandlerNotFoundException("No handler found for request type", requestType);

        return (Task<TResponse>) cached
            .HandlerMethod
            .Invoke(
                handler,
                new object[]
                {
                    request, 
                    cancellationToken
                }
            )!;

    }

    private HandlerDelegate<TResponse> GetHandler<TResponse>(Type requestType)
    {
        Type handlerType = InvocationHandlerType
            .MakeGenericType(requestType, typeof(TResponse));

        object? handler = _serviceProvider
            .GetService(handlerType);

        // Todo: Remove with obsolete code
        if (handler is null)
        {
            Type obsoleteHandlerType = RequestHandlerType
                .MakeGenericType(requestType, typeof(TResponse));

            handler = _serviceProvider
                .GetService(obsoleteHandlerType);
        }
        
        if (handler is null)
            throw new HandlerNotFoundException("No handler found for request type", requestType);

        MethodInfo methodInfo = handlerType
            .GetMethod(
                HandlerMethodName
            )!;
 
        return (
            request,
            cancellationToken
        ) => (Task<TResponse>) methodInfo
            .Invoke(
                handler!,
                new object[]
                {
                    request, 
                    cancellationToken
                }
            )!;
    }
    private CachedHandlerItem GetCachedHandler<TResponse>(Type requestType)
    {   
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        return _handlerCache
            .GetOrAdd(requestType, requestTypeKey =>
            {
                Type handlerType = InvocationHandlerType
                    .MakeGenericType(requestTypeKey, typeof(TResponse));

                MethodInfo handlerMethod = handlerType
                    .GetMethod(HandlerMethodName)!;
                
                return new CachedHandlerItem(
                    _handlerCache,
                    requestType,
                    handlerType,
                    handlerMethod,
                    cleanupTimeout,
                    keepCacheForEver
                );
            });
    }
}