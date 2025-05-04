using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Contracts;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common.Requests;

namespace RouteDispatcher.Tests.net9;

public sealed class DispatcherSendVoidTests
{
    [Fact]
    public async Task Send_WithVoidRequest_ShouldCallHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddRouteDispatcher(
            typeof(TestVoidRequest).Assembly
        );
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        
        var request = new TestVoidRequest
        {
            Content = "Test content: "
        };
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        // Act
        await dispatcher.Send(request, CancellationToken.None);
        
        // Assert
        Assert.True(request.WasHandled);
        Assert.Contains("Handled by TestVoidRequestHandler", request.Content);
    }
    
    [Fact]
    public async Task Send_WithVoidRequest_ShouldAllowCancellation()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddRouteDispatcher(
            typeof(TestVoidRequest).Assembly
        );
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        var request = new TestVoidRequest();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            dispatcher.Send(request, cts.Token));
    }
    
    [Fact]
    public async Task Send_WithMissingHandler_ShouldThrowHandlerNotFoundException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(
            typeof(TestVoidNoHandlerRequest).Assembly
        );
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        var request = new TestVoidNoHandlerRequest();
        
        // Act & Assert
        var exception = await Assert
            .ThrowsAsync<HandlerNotFoundException>(() => 
                dispatcher.Send(request)
            );
        
        Assert.Contains("No handler found", exception.Message);
    }
    
    [Fact]
    public async Task Send_MultipleCalls_ShouldWorkIndependently()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouteDispatcher(
            typeof(TestVoidNoHandlerRequest).Assembly
        );
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        var request1 = new TestVoidRequest { Content = "Request 1: " };
        var request2 = new TestVoidRequest { Content = "Request 2: " };
        
        // Act
        await dispatcher.Send(request1);
        await dispatcher.Send(request2);
        
        // Assert
        Assert.True(request1.WasHandled);
        Assert.True(request2.WasHandled);
        Assert.Contains("Handled by TestVoidRequestHandler", request1.Content);
        Assert.Contains("Handled by TestVoidRequestHandler", request2.Content);
    }
}
