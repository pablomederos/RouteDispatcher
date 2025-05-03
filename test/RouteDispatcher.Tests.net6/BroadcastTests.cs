using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Contracts;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common.Handlers;
using RouteDispatcher.Tests.Common.Messages;

namespace RouteDispatcher.Tests.net6
{
    public class BroadcastTests
    {
        [Fact]
        public async Task Broadcast_SingleMessage_HandlesSuccessfully()
        {
            // Arrange
            const string messageContent = "Test Message";
            var services = new ServiceCollection();
            
            services.AddRouteDispatcher(typeof(TestBroadcastMessage).Assembly);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider
                .GetRequiredService<IDispatcher>();
            var message = new TestBroadcastMessage { Content = messageContent };

            // Act
            await dispatcher.Broadcast(message, CancellationToken.None);

            // Assert
            Assert.Equal($"Processed: {messageContent}", message.Content);
        }

        [Fact]
        public async Task Broadcast_MultipleHandlers_AllHandlersInvoked()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddRouteDispatcher(typeof(TestBroadcastMessage).Assembly);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            var message = new TestMultiBroadcastMessage { Content = "Processed by: " };
            
            // Act
            await dispatcher.Broadcast(message, CancellationToken.None);

            // Assert
            Assert.Contains(nameof(TestMultiBroadcast1MessageHandler), message.Content);
            Assert.Contains(nameof(TestMultiBroadcast2MessageHandler), message.Content);
            Assert.Contains(nameof(TestMultiBroadcast3MessageHandler), message.Content);
        }

        [Fact]
        public async Task Broadcast_NoHandler_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRouteDispatcher(typeof(TestBroadcastMessage).Assembly);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            var message = new TestNoHandlerBroadcastMessage { Content = "No Handler Test" };
        
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                dispatcher.Broadcast(message, CancellationToken.None));
        }
        
        [Fact]
        public async Task Broadcast_CancelledToken_CancelsOperation()
        {
            // Arrange
            var services = new ServiceCollection();
            var message = new TestMultiBroadcastMessage { Content = "Cancelled message" };
            
            services.AddRouteDispatcher(typeof(TestBroadcastMessage).Assembly);
            
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
            // Create a pre-cancelled token
            using var cts = new CancellationTokenSource();
            cts.Cancel();
        
            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                dispatcher.Broadcast(message, cts.Token));
            
            // Assert
            Assert.DoesNotContain(nameof(TestMultiBroadcast1MessageHandler), message.Content);
            Assert.DoesNotContain(nameof(TestMultiBroadcast2MessageHandler), message.Content);
            Assert.DoesNotContain(nameof(TestMultiBroadcast3MessageHandler), message.Content);
        }

        [Fact]
        public async Task Broadcast_HandlerThrowsException_AggregatesExceptions()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddRouteDispatcher(opts =>
            {
                opts.IgnoreBroadcastFailuresUntilTheEnd = true;
                opts.Assemblies = new [] { typeof(TestExceptionBroadcastMessage).Assembly };
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            var message = new TestExceptionBroadcastMessage();
        
            // Act & Assert
            var exception = await Assert.ThrowsAsync<AggregateException>(() => 
                dispatcher.Broadcast(message, CancellationToken.None));
            
            Assert.Equal(3, exception.InnerExceptions.Count);
        }
        
        [Fact]
        public async Task Broadcast_HandlerThrowsException_FailsFast()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddRouteDispatcher(opts =>
            {
                opts.IgnoreBroadcastFailuresUntilTheEnd = false;
                opts.Assemblies = new [] { typeof(TestExceptionBroadcastMessage).Assembly };
            });
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            var message = new TestExceptionBroadcastMessage();
        
            // Act & Assert
            var exception = await Assert.ThrowsAsync<AggregateException>(() => 
                dispatcher.Broadcast(message, CancellationToken.None));
            
            Assert.Equal(1, exception.InnerExceptions.Count);
        }
        
        [Fact]
        public async Task Broadcast_ConcurrentMessages_AllProcessed()
        {
            // Arrange
            var services = new ServiceCollection();
            var controller = new TestMulticastHandledControl();
            
            services.AddRouteDispatcher(typeof(TestMulticastBroadcastMessage).Assembly);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            var messages = Enumerable.Range(1, 10)
                .Select(_ => new TestMulticastBroadcastMessage
                {
                    Controller = controller
                })
                .ToArray();
        
            // Act
            var tasks = messages.Select(m => dispatcher.Broadcast(m, CancellationToken.None));
            await Task.WhenAll(tasks);
        
            // Assert
            Assert.Equal(messages.Length, controller.HandledCount);
        }
    }
}