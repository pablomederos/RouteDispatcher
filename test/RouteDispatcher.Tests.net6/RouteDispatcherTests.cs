using RouteDispatcher.ConcreteServices;
using RouteDispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common;

namespace RouteDispatcher.Tests.net6;

public class RouteDispatcherTests
{

    [Fact]
    public async Task Send_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();

        // Act
        var response = await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal("Test Response", response);
    }

    [Fact]
    public async Task Send_NoHandler_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request, CancellationToken.None));
    }

    [Fact]
    public async Task Send_DifferentRequest_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new OtherRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request, CancellationToken.None));
    }

    [Fact]
    public async Task AddRouteDispatcher_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(typeof(TestRequestHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest();

        // Act
        var response = await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal("Test Response", response);
    }
    [Fact]
    public async Task Send_ValidRequest_NoCancellationToken_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();

        // Act
        var response = await mediator.Send(request);

        // Assert
        Assert.Equal("Test Response", response);
    }

    [Fact]
    public async Task Send_CancelledCancellationToken_DoesNotExecuteHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => mediator.Send(request, cancellationToken));
    }
}