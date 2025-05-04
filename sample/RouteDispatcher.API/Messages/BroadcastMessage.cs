using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Messages
{
    /// <summary>
    /// Represents a message that can be broadcasted to multiple handlers
    /// </summary>
    public class BroadcastMessage : IMessage
    {
        public string Subject { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }
        
        
        public BroadcastMessage(string subject, string content)
        {
            Subject = subject;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }
    }
}
