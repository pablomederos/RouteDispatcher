using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.RequestHandlers
{
    public class GetCustomMessageHandler : IInvocationHandler<GetCustomMessageRequest, string>
    {
        public Task<string> Handle(GetCustomMessageRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Custom message: {request.Content}");
        }
    }
}
