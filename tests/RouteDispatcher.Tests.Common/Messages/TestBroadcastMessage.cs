using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Messages;

/// <summary>
/// Test message class for broadcast tests
/// </summary>
public sealed class TestBroadcastMessage : IMessage
{
    public string Content { get; set; } = string.Empty;
}