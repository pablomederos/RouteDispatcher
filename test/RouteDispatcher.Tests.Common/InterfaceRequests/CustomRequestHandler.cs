using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.InterfaceRequests
{
    // Handler for the custom request
    public class CustomRequestHandler : IRequestHandler<CustomRequest, string>
    {
        public Task<string> Handle(CustomRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Processed: {request.Content}");
        }
    }
}
