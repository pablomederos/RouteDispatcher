using Microsoft.AspNetCore.Mvc;
using RouteDispatcher.API.Messages;
using RouteDispatcher.API.Models;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BroadcastController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<BroadcastController> _logger;

        public BroadcastController(IDispatcher dispatcher, ILogger<BroadcastController> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        /// <summary>
        /// Broadcasts a message to all registered handlers
        /// </summary>
        /// <param name="request">The broadcast message request</param>
        /// <returns>Accepted status code indicating the message was accepted for broadcasting</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Post([FromBody] BroadcastRequest request)
        {
            _logger.LogInformation("Broadcasting message: {Subject}", request.Subject);
            
            // Create a broadcast message
            var message = new BroadcastMessage(request.Subject, request.Content);
            
            // Broadcast to all handlers
            await _dispatcher.Broadcast(message);
            
            // Return 202 Accepted since this is an asynchronous operation
            return Accepted();
        }
    }
}
