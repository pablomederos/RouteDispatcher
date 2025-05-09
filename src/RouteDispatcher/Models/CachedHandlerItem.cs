using System;
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
            TimeSpan cleanTimeout,
            bool keepCacheForEver
        )
        {
            HandlerType = handlerType;
            if (keepCacheForEver) return;
            
            _timeout = new Timer
            {
                Interval = cleanTimeout.TotalMilliseconds
            };
            _timeout.Elapsed += (_, _) =>
            {
                _isDisposed = true;
                container.TryRemove(requestType);
                _timeout.Dispose();
            };

            _timeout.Start();
        }

        public Type HandlerType { get; }
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