using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Messages;

public sealed class TestMultiBroadcastMessage : IMessage
{
    public string Content { get; set; } = string.Empty;
}