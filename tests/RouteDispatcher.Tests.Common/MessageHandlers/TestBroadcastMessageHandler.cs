using RouteDispatcher.Contracts;
using RouteDispatcher.Tests.Common.Messages;

namespace RouteDispatcher.Tests.Common.Handlers
{
    /// <summary>
    /// Test message handler for broadcast tests
    /// </summary>
    public sealed class TestBroadcastMessageHandler : IMessageHandler<TestBroadcastMessage>
    {
        public Task OnMessage(TestBroadcastMessage message, CancellationToken cancellationToken = default)
        {
            message.Content = $"Processed: {message.Content}";
            return Task.CompletedTask;
        }
    }
}
