using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Handlers
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class GetMessageRequestHandler : IRequestHandler<GetMessageRequest, string>
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public Task<string> Handle(GetMessageRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Simulate some processing
            return Task.FromResult("Hello from the mediator!");
        }
    }
}