using System;
using RouteDispatcher.Contracts;
using RouteDispatcher.Models;

namespace RouteDispatcher.ConcreteServices;

#pragma warning disable CS0618 // Type or member is obsolete
public sealed partial class Dispatcher : IMediator, IDispatcher
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHandlerCache? _handlerCache;
    private readonly DispatcherConfiguration _configurationOptions;

    public Dispatcher(IServiceProvider serviceProvider, DispatcherConfiguration configurationOptions)
    {
        _serviceProvider = serviceProvider;
        _configurationOptions = configurationOptions;
        
        if(_configurationOptions.UseHandlersCache)
            _handlerCache = _serviceProvider
                .GetService(typeof(IHandlerCache)) as IHandlerCache
                ?? throw new InvalidOperationException("Handler cache service is not registered.");
    }
}