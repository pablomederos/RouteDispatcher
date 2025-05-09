using System;

namespace RouteDispatcher.Models;

public sealed class CachedTypeConfiguration
{
    public Type RequestType { get; set; } = null!;
    public Type HandlerType { get; set; } = null!;
    public TimeSpan CleanTimeout { get; set; } = TimeSpan.Zero;
    public bool KeepCacheForEver { get; set; } = false;
}