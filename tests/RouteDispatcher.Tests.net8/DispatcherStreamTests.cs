using Microsoft.Extensions.DependencyInjection;
using RouteDispatcher.Contracts;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Extensions;
using RouteDispatcher.Tests.Common.Stream;

namespace RouteDispatcher.Tests.net8
{
    public class DispatcherStreamTests
    {
        private readonly IDispatcher _dispatcher;

        public DispatcherStreamTests()
        {
            var services = new ServiceCollection();
            
            services.AddRouteDispatcher(
                typeof(StreamResponse).Assembly
            );
            
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            
            _dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            StreamRequestHandler.ResetCounter();
        }

        [Fact]
        public async Task Stream_ShouldReturnItems_WhenHandlerIsRegistered()
        {
            // Arrange
            var request = new StreamRequest(3);
            var results = new List<StreamResponse>();

            // Act
            await foreach (StreamResponse response in _dispatcher.Stream(request))
            {
                results.Add(response);
            }

            // Assert
            Assert.Equal(3, results.Count);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(i, results[i].Index);
                Assert.Equal($"Item {i}", results[i].Value);
            }
        }

        [Fact]
        public async Task Stream_ShouldThrowHandlerNotFoundException_WhenHandlerNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddRouteDispatcher();
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            var request = new StreamRequest(3);

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
            {
                await foreach (StreamResponse _ in dispatcher.Stream(request))
                {
                    // This code should not run
                }
            });
        }

        [Fact]
        public async Task Stream_ShouldRespectCancellationToken()
        {
            // Arrange
            var request = new StreamRequest(10, delayMs: 20);
            var cts = new CancellationTokenSource();
            var results = new List<StreamResponse>();

            // Act & Assert
            Task task = Task.Run(async () =>
            {
                await foreach (StreamResponse response in _dispatcher.Stream(request, cts.Token))
                {
                    results.Add(response);
                    if (results.Count >= 3)
                    {
                        cts.Cancel();
                    }
                }
            }, cts.Token);

            // Either task is canceled or completes with fewer than expected items
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // Expected when cancellation happens
                Assert.IsType<OperationCanceledException>(ex);
            }

            Assert.True(results.Count < 10, "Stream should have been canceled before completing");
        }

        [Fact]
        public async Task Stream_ShouldPropagateExceptionsFromHandler()
        {
            // Arrange
            var request = new StreamRequest(5, throwException: true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (StreamResponse _ in _dispatcher.Stream(request))
                {
                    // This will throw after the first item
                }
            });

            Assert.Equal("Test exception from stream handler", exception.Message);
        }

        [Fact]
        public async Task Stream_ShouldUseCachedHandler_WhenCachingIsEnabled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRouteDispatcher(options =>
            {
                options.Assemblies = [ typeof(StreamRequestHandler).Assembly ];
                options.UseHandlersCache = true;
                options.KeepCacheForEver = true;
            });
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            StreamRequestHandler.ResetCounter();
            
            // Act
            // First call
            var request1 = new StreamRequest(3);
            var results1 = new List<StreamResponse>();
            await foreach (StreamResponse response in dispatcher.Stream(request1))
            {
                results1.Add(response);
            }
            
            // Second call with same request type
            var request2 = new StreamRequest(2);
            var results2 = new List<StreamResponse>();
            await foreach (StreamResponse response in dispatcher.Stream(request2))
            {
                results2.Add(response);
            }

            // Assert
            Assert.Equal(3, results1.Count);
            Assert.Equal(2, results2.Count);
            Assert.Equal(2, StreamRequestHandler.HandleCallCount);
        }

        [Fact]
        public async Task Stream_ShouldNotCacheHandler_WhenCachingIsDisabled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRouteDispatcher(options =>
            {
                options.Assemblies = [ typeof(StreamRequestHandler).Assembly ];
                options.UseHandlersCache = false;
            });
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            StreamRequestHandler.ResetCounter();
            
            // Act - Make multiple requests
            for (int i = 0; i < 3; i++)
            {
                var request = new StreamRequest(1);
                await foreach (StreamResponse _ in dispatcher.Stream(request))
                {
                    // Just consume the stream
                }
            }

            // Assert - Handler lookup should happen for each request
            Assert.Equal(3, StreamRequestHandler.HandleCallCount);
        }

        [Fact]
        public async Task Stream_WithCacheAndCleanupTimeout_ShouldRefreshCache()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddRouteDispatcher(options =>
            {
                options.Assemblies = [ typeof(StreamRequestHandler).Assembly ];
                options.UseHandlersCache = true;
                options.KeepCacheForEver = false;
                options.DiscardCachedHandlersTimeout = TimeSpan.FromMilliseconds(100);
            });
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
            
            // Act
            var request = new StreamRequest(1);
            
            // First call populates cache
            await foreach (StreamResponse _ in dispatcher.Stream(request))
            {
                // Just consume the stream
            }
            
            // Wait for cache to potentially expire
            await Task.Delay(150);
            
            // Second call should still work
            var results = new List<StreamResponse>();
            await foreach (StreamResponse response in dispatcher.Stream(request))
            {
                results.Add(response);
            }

            // Assert
            Assert.Single(results);
        }
    }
}
