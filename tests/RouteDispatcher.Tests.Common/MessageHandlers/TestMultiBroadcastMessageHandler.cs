using RouteDispatcher.Contracts;
using RouteDispatcher.Tests.Common.Messages;

namespace RouteDispatcher.Tests.Common.Handlers;

public sealed class TestMultiBroadcast1MessageHandler : IMessageHandler<TestMultiBroadcastMessage>
{
    public Task OnMessage(TestMultiBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        message.Content += $"{ nameof(TestMultiBroadcast1MessageHandler)}; ";
        return Task.CompletedTask;
    }
}
public sealed class TestMultiBroadcast2MessageHandler : IMessageHandler<TestMultiBroadcastMessage>
{
    public Task OnMessage(TestMultiBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        message.Content += $"{ nameof(TestMultiBroadcast2MessageHandler)}; ";
        return Task.CompletedTask;
    }
}
public sealed class TestMultiBroadcast3MessageHandler : IMessageHandler<TestMultiBroadcastMessage>
{
    public Task OnMessage(TestMultiBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        message.Content += $"{ nameof(TestMultiBroadcast3MessageHandler)}; ";
        return Task.CompletedTask;
    }
}