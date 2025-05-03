using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Handlers
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class GetCustomMessageHandler : IRequestHandler<GetCustomMessageRequest, string>
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public Task<string> Handle(GetCustomMessageRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Custom message: {request.Content}");
        }
    }
}
