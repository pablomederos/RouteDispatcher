using System;
using System.Reflection;

namespace RouteDispatcher.Models
{
    public sealed class DispatcherConfiguration
    {
        private Assembly[] _assemblies = Array.Empty<Assembly>();
        private TimeSpan _discardCachedHandlersTimeout = TimeSpan.FromSeconds(30);



        public bool UseHandlersCache { get; set; } = false;
        public bool KeepCacheForEver { get; set; } = false;
        public TimeSpan DiscardCachedHandlersTimeout
        {
            get => _discardCachedHandlersTimeout;
            set
            {
                if (value == TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("DiscardCachedHandlersTimeout", "Timeout cannot be zero");

                _discardCachedHandlersTimeout = value;
            }
        }
        
        public Assembly[] Assemblies
        {
            get => _assemblies;
            set
            {
                if (value == null || value.Length == 0)
                    return;

                foreach (var assembly in value)
                {
                    if (assembly == null)
                        throw new ArgumentNullException("assembly", "Assembly argument cannot be null");
                }

                _assemblies = value;
            }
        }
    }
}