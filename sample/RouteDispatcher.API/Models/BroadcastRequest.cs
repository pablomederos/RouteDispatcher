namespace RouteDispatcher.API.Models;

/// <summary>
/// Request model for broadcasting messages
/// </summary>
public class BroadcastRequest
{
    public string Subject { get; init; } = string.Empty;
        
    public string Content { get; init; } = string.Empty;
}