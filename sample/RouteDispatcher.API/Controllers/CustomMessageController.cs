using Microsoft.AspNetCore.Mvc;
using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomMessageController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public CustomMessageController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetCustomMessageRequest request)
        {
            var result = await _dispatcher.Send(request);
            return Ok(result);
        }
    }
}
