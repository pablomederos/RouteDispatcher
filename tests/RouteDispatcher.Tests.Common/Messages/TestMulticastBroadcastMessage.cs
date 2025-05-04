using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Messages;

/// <summary>
/// Test message class for broadcast tests
/// </summary>

public sealed class TestMulticastHandledControl
{
    private int _handledCount;
    public void IncrementHandledCount() => Interlocked.Add(ref _handledCount, 1);
    
    public int HandledCount { get => _handledCount; }
}
public sealed class TestMulticastBroadcastMessage : IMessage
{
    public TestMulticastHandledControl Controller { get; set; } = null!;
}