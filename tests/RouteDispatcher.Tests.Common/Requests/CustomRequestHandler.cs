using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Requests
{
    // Handler for the custom request
    [Obsolete("Obsolete")]
    public class CustomRequestHandler : IRequestHandler<CustomRequest, string>
    {
        public Task<string> Handle(CustomRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Processed: {request.Content}");
        }
    }
}
