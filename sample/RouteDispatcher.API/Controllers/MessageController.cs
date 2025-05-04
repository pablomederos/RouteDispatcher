using Microsoft.AspNetCore.Mvc;
using RouteDispatcher.API.Models;

namespace RouteDispatcher.API.Controllers
{
    [ApiController]
    [Route("Request/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string message = await _messageService.GetMessage();
            return Ok(message);
        }
        
        /// <summary>
        /// Fire-and-forget endpoint to log user actions
        /// </summary>
        /// <param name="action">The action to log</param>
        /// <returns>202 Accepted when the operation is being processed</returns>
        [HttpPost("log")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> LogAction([FromBody] RequestAction action)
        {
            // Process the fire-and-forget request
            await _messageService.LogAction(action.Content);
            
            // Return 202 Accepted to indicate the request was accepted for processing
            return Accepted();
        }
    }
}