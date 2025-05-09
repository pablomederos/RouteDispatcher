using System;
using System.Linq.Expressions;
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
        {
            HandlerDelegate<TResponse> handler = GetHandler<TResponse>(requestType);
            return handler(request, _serviceProvider, cancellationToken);
        }

        HandlerDelegate<TResponse> cachedHandlerType = GetCachedHandler<TResponse>(requestType) 
                                                 ?? throw new HandlerNotFoundException("No handler found for request type", requestType);

        return cachedHandlerType(request, _serviceProvider, cancellationToken);
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
            _,
            cancellationToken
        ) => (Task<TResponse>) methodInfo.Invoke(
            handler!,
            new object[]
            {
                request, 
                cancellationToken
            }
        )!;
    }
    private HandlerDelegate<TResponse> GetCachedHandler<TResponse>(Type requestType)
    {
        if(!_configurationOptions.UseHandlersCache)
            throw new InvalidOperationException("Handlers cache is disabled.");
        
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        CachedHandlerItem cached = _handlerCache
            !.GetOrAdd(requestType, requestTypeKey =>
            {
                Type handlerType = InvocationHandlerType
                    .MakeGenericType(requestTypeKey, typeof(TResponse));
                
                return new CachedHandlerItem(
                    _handlerCache!,
                    requestType,
                    handlerType,
                    cleanupTimeout,
                    keepCacheForEver
                );
            });

        if (!keepCacheForEver)
            cached.Refresh(cleanupTimeout);

        object handler = _serviceProvider
            .GetService(cached.HandlerType)
            ?? throw new HandlerNotFoundException("No handler found for request type", requestType);

        MethodInfo method = cached
            .HandlerType
            .GetMethod(HandlerMethodName)!;

        return (request, _, cancellationToken) 
            =>  (Task<TResponse>) method.Invoke(
            handler,
            new object[]
            {
                request, 
                cancellationToken
            }
        )!;
    }
}