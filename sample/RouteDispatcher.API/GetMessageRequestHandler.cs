using RouteDispatcher.Contracts;

namespace RouteDispatcher.API
{
    public class GetMessageRequestHandler : IRequestHandler<GetMessageRequest, string>
    {
        public Task<string> Handle(GetMessageRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Simulate some processing
            return Task.FromResult("Hello from the mediator!");
        }
    }
}