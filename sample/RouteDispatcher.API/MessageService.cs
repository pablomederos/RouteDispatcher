using RouteDispatcher.API.Requests;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API
{
    public class MessageService
    {
        private readonly IDispatcher _dispatcher;

        public MessageService(IDispatcher mediator)
        {
            _dispatcher = mediator;
        }

        public async Task<string> GetMessage()
        {
            return await _dispatcher.Send(new GetMessageRequest());
        }

        public async Task LogAction(string actionDescription)
        {
            // Usa la implementación de IRequest (sin retorno específico)
            await _dispatcher.Send(new LogActionRequest(actionDescription));
        }
    }
}