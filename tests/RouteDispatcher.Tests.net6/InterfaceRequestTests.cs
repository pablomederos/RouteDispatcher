using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Contracts;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common.Requests;
#pragma warning disable CS0618 // Type or member is obsolete

namespace RouteDispatcher.Tests.net6
{
    public class InterfaceRequestTests
    {
        [Fact]
        public async Task Dispatcher_ShouldResolveHandler_ForRequestImplementingCustomInterface()
        {
            // Arrange
            var services = new ServiceCollection();
            string message = "Hello from interface-based request";
            
            services.AddRouteDispatcher(typeof(CustomRequestHandler).Assembly);
            
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            var request = new CustomRequest { Content = message };
            
            // Act
            string result = await dispatcher.Send(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal($"Processed: {message}", result);
        }
    }
}
