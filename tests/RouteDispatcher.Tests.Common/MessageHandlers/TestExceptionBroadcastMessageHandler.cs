using RouteDispatcher.Contracts;
using RouteDispatcher.Tests.Common.Messages;

namespace RouteDispatcher.Tests.Common.MessageHandlers;

/// <summary>
/// Test message handler for broadcast tests
/// </summary>
public sealed class TestException1BroadcastMessageHandler : IMessageHandler<TestExceptionBroadcastMessage>
{
    public Task OnMessage(TestExceptionBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        throw new Exception("Test exception");
    }
}

public sealed class TestException2BroadcastMessageHandler : IMessageHandler<TestExceptionBroadcastMessage>
{
    public Task OnMessage(TestExceptionBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        throw new Exception("Test exception");
    }
}

public sealed class TestException3BroadcastMessageHandler : IMessageHandler<TestExceptionBroadcastMessage>
{
    public Task OnMessage(TestExceptionBroadcastMessage message, CancellationToken cancellationToken = default)
    {
        throw new Exception("Test exception");
    }
}