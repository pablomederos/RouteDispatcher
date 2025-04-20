using System;
using System.Timers;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Models
{
    public sealed record CompiledAutocleanDelegate<TResponse>
    {
        public CompiledAutocleanDelegate(
            IHandlerCache container,
            Type requestType,
            HandlerDelegate<TResponse> value,
            TimeSpan cleanTimeout,
            bool keepCacheForEver
        )
        {
            Value = value;
            if (!keepCacheForEver)
            { 
                _timeout = new Timer
                {
                    Interval = cleanTimeout.TotalMilliseconds
                };
                _timeout.Elapsed += (sender, args) =>
                {
                    _isDisposed = true;
                    container.TryRemove(requestType);
                    _timeout.Dispose();
                };

                _timeout.Start();
            }
        }

        public HandlerDelegate<TResponse> Value { get; }
        private readonly Timer _timeout;
        private bool _isDisposed = false;

        public void Refresh(TimeSpan timeSpan = default)
        {
            if(_isDisposed || _timeout == null)
                return;

            _timeout.Interval = timeSpan == default
                ? _timeout.Interval
                : timeSpan.TotalMilliseconds;
        }
    }
}