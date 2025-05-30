using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Extensions;
using RouteDispatcher.Contracts;
using RouteDispatcher.Models;
using RouteDispatcher.Tests.Common;

namespace RouteDispatcher.Tests.net8;

public class DispatcherConfigurationTests
{
    [Fact]
    public void AddRouteDispatcher_WithConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(DispatcherConfigurationTests).Assembly ];
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddRouteDispatcher_WithMultipleAssemblies_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(DispatcherConfigurationTests).Assembly, typeof(IDispatcher).Assembly ];
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddRouteDispatcher_WithNullConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection FailedAssign() => services.AddRouteDispatcher((Action<DispatcherConfiguration>)null!);

        // Assert
        Assert.Throws<ArgumentNullException>((Func<IServiceCollection>)FailedAssign);
    }

    [Fact]
    public void AddRouteDispatcher_WithNoAssemblies_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [];
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddRouteDispatcher_WithZeroTimeout_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            services.AddRouteDispatcher(options =>
            {
                options.DiscardCachedHandlersTimeout = TimeSpan.Zero;
            });
        });
    }

    [Fact]
    public async Task AddRouteDispatcher_UseCacheTrue_CacheIsUsed()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = true;
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var handlerCache = serviceProvider.GetRequiredService<IHandlerCache>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);

        // Assert
        Assert.False(handlerCache.IsEmpty());
    }

    [Fact]
    public void AddRouteDispatcher_UseCacheFalse_CacheIsNotUsed()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = false;
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        var handlerCache = serviceProvider.GetService<IHandlerCache>();

        // Assert
        Assert.Null(handlerCache);
    }

    [Fact]
    public async Task AddRouteDispatcher_Timeout_CacheIsCleaned()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = true;
            options.DiscardCachedHandlersTimeout = TimeSpan.FromSeconds(1);
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var handlerCache = serviceProvider.GetRequiredService<IHandlerCache>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(handlerCache.IsEmpty());
    }

    [Fact]
    public async Task AddRouteDispatcher_Timeout_HandlerIsReused_CacheIsKept()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = true;
            options.DiscardCachedHandlersTimeout = TimeSpan.FromSeconds(1.1);
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var handlerCache = serviceProvider.GetRequiredService<IHandlerCache>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        Assert.False(handlerCache.IsEmpty());
    }

    [Fact]
    public async Task AddRouteDispatcher_Timeout_HandlerIsReused_CacheIsCleanedAfterSecondTimeout()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = true;
            options.DiscardCachedHandlersTimeout = TimeSpan.FromSeconds(1);
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var handlerCache = serviceProvider.GetRequiredService<IHandlerCache>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(handlerCache.IsEmpty());
    }

    [Fact]
    public async Task AddRouteDispatcher_KeepCacheForEver_CacheIsNeverCleaned()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddRouteDispatcher(options =>
        {
            options.Assemblies = [ typeof(TestRequest).Assembly ];
            options.UseHandlersCache = true;
            options.KeepCacheForEver = true;
            options.DiscardCachedHandlersTimeout = TimeSpan.FromSeconds(1);
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var handlerCache = serviceProvider.GetRequiredService<IHandlerCache>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(handlerCache.IsEmpty());
    }
}