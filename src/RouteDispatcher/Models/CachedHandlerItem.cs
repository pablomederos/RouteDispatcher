using System;
using System.Reflection;
using System.Timers;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Models
{
    internal sealed class CachedHandlerItem
    {
        public CachedHandlerItem(
            IHandlerCache container,
            Type requestType,
            Type handlerType,
            MethodInfo handlerMethod,
            TimeSpan cleanTimeout,
            bool keepCacheForEver
        )
        {
            HandlerType = handlerType;
            HandlerMethod = handlerMethod;
            if (keepCacheForEver) return;
            
            _timeout = new Timer
            {
                Interval = cleanTimeout.TotalMilliseconds
            };
            _timeout.Elapsed += (_, _) =>
            {
                container.TryRemove(requestType);
                _timeout.Dispose();
                _isDisposed = true;
            };

            _timeout.Start();
        }

        public Type HandlerType { get; }
        public MethodInfo HandlerMethod { get; }
        private readonly Timer? _timeout;
        private bool _isDisposed;

        public void Refresh(TimeSpan timeSpan = default)
        {
            if(_isDisposed || _timeout == null)
                return;

            _timeout.Interval = timeSpan == TimeSpan.Zero
                ? _timeout.Interval
                : timeSpan.TotalMilliseconds;
        }
    }
}