using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Requests
{
    /// <summary>
    /// Request to log an action without expecting a specific response.
    /// Implements IRequest for fire-and-forget operations
    /// </summary>
    public class LogActionRequest : IRequest
    {
        public string Action { get; }
        public DateTime Timestamp { get; }

        public LogActionRequest(string action)
        {
            Action = action;
            Timestamp = DateTime.UtcNow;
        }
    }
}
