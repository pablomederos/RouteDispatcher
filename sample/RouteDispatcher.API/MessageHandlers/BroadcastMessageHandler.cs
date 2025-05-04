using RouteDispatcher.API.Messages;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.MessageHandlers
{
    /// <summary>
    /// Handler for processing broadcast messages
    /// </summary>
    public class BroadcastMessageHandler : IMessageHandler<BroadcastMessage>
    {
        private readonly ILogger<BroadcastMessageHandler> _logger;

        public BroadcastMessageHandler(ILogger<BroadcastMessageHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processes the broadcast message
        /// </summary>
        /// <param name="message">The message to process</param>
        /// <param name="cancellation">A cancellation token. Default to None</param>
        public Task OnMessage(BroadcastMessage message, CancellationToken cancellation)
        {
            _logger.LogInformation(
                "Broadcast message received - Subject: {Subject}, Content: {Content}, Timestamp: {Timestamp}",
                message.Subject,
                message.Content,
                message.Timestamp);
            
            return Task.CompletedTask;
        }
    }
}
