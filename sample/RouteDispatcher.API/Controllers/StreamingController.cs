using Microsoft.AspNetCore.Mvc;
using RouteDispatcher.API.Models;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamingController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public StreamingController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("data")]
    public IAsyncEnumerable<DataStreamResponse> StreamData(
        [FromBody] DataStreamRequest request, 
        CancellationToken cancellationToken)
    {
        return _dispatcher.Stream(request, cancellationToken);
    }

    [HttpGet("data")]
    public IAsyncEnumerable<DataStreamResponse> StreamDataGet(
        [FromQuery] int count = 10, 
        [FromQuery] string? filter = null, 
        CancellationToken cancellationToken = default)
    {
        var request = new DataStreamRequest(count, filter);
        return _dispatcher.Stream(request, cancellationToken);
    }
}
