using System;
using System.Collections.Concurrent;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.ConcreteServices
{
    public sealed class HandlerCacheService : IHandlerCache
    {
        private readonly ConcurrentDictionary<Type, Delegate> _handlerCache = new();

        public CompiledHandlerCaller<TResponse> GetOrAdd<TResponse>(Type requestType, Func<Type, CompiledHandlerCaller<TResponse>> value)
            => (CompiledHandlerCaller<TResponse>) _handlerCache.GetOrAdd(requestType, value);
    }
}