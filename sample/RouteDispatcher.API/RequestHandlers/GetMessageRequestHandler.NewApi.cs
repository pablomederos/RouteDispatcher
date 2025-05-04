using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.RequestHandlers
{
    public class GetMessageRequestHandler : IInvocationHandler<GetMessageRequest, string>
    {
        public Task<string> Handle(GetMessageRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Simulate some processing
            return Task.FromResult("Hello from the mediator!");
        }
    }
}