using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using RouteDispatcher.Contracts;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Models;

namespace RouteDispatcher.ConcreteServices;

public sealed partial class Dispatcher
{
    public IAsyncEnumerable<TResponse> Stream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Type requestType = request.GetType();

        if (!_configurationOptions.UseHandlersCache)
            return GetStreamHandler<TResponse>(requestType)
                (request, cancellationToken);
        
        CachedHandlerItem cached = GetCachedStreamHandler<TResponse>(requestType);
        
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        if (!keepCacheForEver)
            cached.Refresh(cleanupTimeout);

        object handler = _serviceProvider
            .GetService(cached.HandlerType)
            ?? throw new HandlerNotFoundException("No stream handler found for request type", requestType);

        cached
            .HandlerMethod ??= handler.GetType()
            .GetMethod(StreamMethodName)!;
        
        return (IAsyncEnumerable<TResponse>) cached
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

    private StreamHandlerDelegate<TResponse> GetStreamHandler<TResponse>(Type requestType)
    {
        Type handlerType = StreamInvocationHandlerType
            .MakeGenericType(requestType, typeof(TResponse));

        object? handler = _serviceProvider
            .GetService(handlerType);
        
        if (handler is null)
            throw new HandlerNotFoundException("No stream handler found for request type", requestType);

        MethodInfo methodInfo = handlerType
            .GetMethod(
                StreamMethodName
            )!;
 
        return (
            request,
            cancellationToken
        ) => (IAsyncEnumerable<TResponse>) methodInfo
            .Invoke(
                handler,
                new object[]
                {
                    request, 
                    cancellationToken
                }
            )!;
    }
    
    private CachedHandlerItem GetCachedStreamHandler<TResponse>(Type requestType)
    {   
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        return _handlerCache
            .GetOrAdd(requestType, requestTypeKey =>
            {
                Type handlerType = StreamInvocationHandlerType
                    .MakeGenericType(requestTypeKey, typeof(TResponse));

                MethodInfo handlerMethod = handlerType
                    .GetMethod(StreamMethodName)!;
                
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