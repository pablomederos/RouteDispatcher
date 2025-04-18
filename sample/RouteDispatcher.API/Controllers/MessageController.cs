using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RouteDispatcher.API.Controllers;
using RouteDispatcher.API;

namespace RouteDispatcher.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
    }
}