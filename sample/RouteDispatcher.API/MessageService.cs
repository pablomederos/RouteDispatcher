using RouteDispatcher;
using RouteDispatcher.Contracts;
using System.Threading.Tasks;

namespace RouteDispatcher.API
{
    public class MessageService
    {
        private readonly IMediator _mediator;

        public MessageService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> GetMessage()
        {
            return await _mediator.Send(new GetMessageRequest());
        }
    }
}