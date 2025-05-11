using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Models;

namespace RouteDispatcher.Contracts
{
    internal interface IHandlerCache
    {
        CachedHandlerItem GetOrAdd(Type requestType, Func<Type, CachedHandlerItem> value);
        void TryRemove(Type item);
        bool IsEmpty();
    }

    public delegate Task<TResponse> HandlerDelegate<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken
    );
    
    public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken
    );
}