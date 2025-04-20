using RouteDispatcher.ConcreteServices;
using RouteDispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Models;

namespace RouteDispatcher.Tests.net6;

public class RouteDispatcherTests
{
    [Fact]
    public async Task Send_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IHandlerCache, HandlerCacheService>();
        services.AddTransient<IDispatcher, Dispatcher>(it => new Dispatcher(it, new DispatcherConfiguration()));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();

        // Act
        var response = await dispatcher.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal("Test Response", response);
    }

    [Fact]
    public async Task Send_NoHandler_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IHandlerCache, HandlerCacheService>();
        services.AddTransient<IDispatcher, Dispatcher>(it => new Dispatcher(it, new DispatcherConfiguration()));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(request, CancellationToken.None));
    }

    [Fact]
    public async Task Send_DifferentRequest_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IHandlerCache, HandlerCacheService>();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IDispatcher, Dispatcher>(it => new Dispatcher(it, new DispatcherConfiguration()));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new OtherRequest();

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(request, CancellationToken.None));
    }

    [Fact]
    public async Task AddRouteDispatcher_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(typeof(TestRequestHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();

        // Act
        var response = await dispatcher.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal("Test Response", response);
    }

    [Fact]
    public async Task Send_ValidRequest_NoCancellationToken_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(typeof(TestRequestHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();

        // Act
        var response = await dispatcher.Send(request);

        // Assert
        Assert.Equal("Test Response", response);
    }

    [Fact]
    public async Task Send_CancelledCancellationToken_DoesNotExecuteHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(typeof(TestRequestHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => dispatcher.Send(request, cancellationToken));
    }

    [Fact]
    public async Task Send_NewScope_NoException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(typeof(TestRequestHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var request = new TestRequest();

        // Act
        await dispatcher.Send(request);

        using (var scope = serviceProvider.CreateScope())
        {
            var scopedDispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

            // Assert
            await scopedDispatcher.Send(request);
        }
    }
}