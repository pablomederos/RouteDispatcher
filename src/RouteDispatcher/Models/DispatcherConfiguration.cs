using System;
using System.Linq;
using System.Reflection;

namespace RouteDispatcher.Models
{
    public sealed class DispatcherConfiguration
    {
        private Assembly[] _assemblies = Array.Empty<Assembly>();
        private TimeSpan _discardCachedHandlersTimeout = TimeSpan.FromSeconds(30);

        public bool UseHandlersCache { get; set; } = false;
        public bool KeepCacheForEver { get; set; } = false;
        public bool IgnoreBroadcastFailuresUntilTheEnd { get; set; } = false;
        public TimeSpan DiscardCachedHandlersTimeout
        {
            get => _discardCachedHandlersTimeout;
            set
            {
                if (value == TimeSpan.Zero || value.TotalMilliseconds < 0)
                    throw new ArgumentOutOfRangeException(nameof(DiscardCachedHandlersTimeout), "Timeout cannot be zero");

                _discardCachedHandlersTimeout = value;
            }
        }
        
        public Assembly[] Assemblies
        {
            get => _assemblies;
            set
            {
                if (value is not { Length: >  0 })
                    return;

                if (value.Any(assembly => assembly == null))
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentNullException("assembly", "Assembly argument cannot be null");

                _assemblies = value;
            }
        }
    }
}