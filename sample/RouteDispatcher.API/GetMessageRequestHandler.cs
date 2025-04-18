using RouteDispatcher;
using RouteDispatcher.Contracts;
using System.Threading.Tasks;

namespace RouteDispatcher.API
{
    public class GetMessageRequestHandler : IRequestHandler<GetMessageRequest, string>
    {
        public Task<string> Handle(GetMessageRequest request)
        {
            return Task.FromResult("Hello from the mediator!");
        }
    }
}