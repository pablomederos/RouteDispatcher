using Microsoft.Extensions.Logging;
using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;
using RouteDispatcher.Models;
using System.Threading;
using System.Threading.Tasks;

namespace RouteDispatcher.API.RequestHandlers
{
    /// <summary>
    /// Handler for LogActionRequest that implements IInvocationHandler<LogActionRequest, Empty>
    /// </summary>
    public class LogActionHandler : IInvocationHandler<LogActionRequest, Empty>
    {
        private readonly ILogger<LogActionHandler> _logger;

        public LogActionHandler(ILogger<LogActionHandler> logger)
        {
            _logger = logger;
        }

        public Task<Empty> Handle(LogActionRequest request, CancellationToken cancellationToken)
        {
            // Log the action
            _logger.LogInformation("Action logged: {Action} at {Timestamp}", 
                request.Action, 
                request.Timestamp);

            // As this is a fire-and-forget operation, we just return Empty.Value
            // which has an implicit conversion to Task<Empty>
            return Empty.Value;
        }
    }
}
