using RouteDispatcher.Contracts;
using RouteDispatcher.Tests.Common.Messages;

namespace RouteDispatcher.Tests.Common.MessageHandlers
{
    /// <summary>
    /// Test message handler for broadcast tests
    /// </summary>
    public sealed class TestMulticastBroadcastMessageHandler : IMessageHandler<TestMulticastBroadcastMessage>
    {
        public Task OnMessage(TestMulticastBroadcastMessage message, CancellationToken cancellationToken = default)
        {
            message.Controller.IncrementHandledCount();
            return Task.CompletedTask;
        }
    }
}
