using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Models
{
    internal sealed class HandlerCacheService : IHandlerCache
    {
        private readonly ConcurrentDictionary<Type, object> _handlerCache;

        public HandlerCacheService(Dictionary<Type, CachedTypeConfiguration>? cachedTypes)
        {
            _handlerCache = new ConcurrentDictionary<Type, object>();
            
            if (cachedTypes is not { Count: > 0 }) return;

            foreach (var cachedType in cachedTypes)
                _handlerCache[cachedType.Key] = new CachedHandlerItem(
                        container: this,
                        requestType: cachedType.Value.RequestType,
                        handlerType: cachedType.Value.HandlerType,
                        cleanTimeout: cachedType.Value.CleanTimeout,
                        keepCacheForEver: cachedType.Value.KeepCacheForEver
                    );
        }
        
        public CachedHandlerItem GetOrAdd(
            Type requestType, 
            Func<Type, CachedHandlerItem> value
        )
            => (CachedHandlerItem) _handlerCache
                .GetOrAdd(requestType, value);

        public void TryRemove(Type requestType)
            => _handlerCache.TryRemove(requestType, out _);
            
        public bool IsEmpty()
            => _handlerCache.IsEmpty;
    }
}