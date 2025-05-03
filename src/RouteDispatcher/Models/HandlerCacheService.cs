using System;
using System.Collections.Concurrent;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Models
{
    internal sealed class HandlerCacheService : IHandlerCache
    {
        private readonly ConcurrentDictionary<Type, object> _handlerCache = new();

        public CompiledAutocleanDelegate<TResponse> GetOrAdd<TResponse>(Type requestType, Func<Type, CompiledAutocleanDelegate<TResponse>> value)
            => (CompiledAutocleanDelegate<TResponse>)_handlerCache
                .GetOrAdd(requestType, value);

        public void TryRemove(Type requestType)
            => _handlerCache.TryRemove(requestType, out var _);
            
        public bool IsEmpty()
            => _handlerCache.IsEmpty;
    }
}