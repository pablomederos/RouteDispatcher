using System;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.ConcreteServices
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(requestType, typeof(TResponse));

            var handler = _serviceProvider
                .GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for request type {requestType.Name}");

            var methodInfo = handlerType.GetMethod("Handle");
            
            var result = await (Task<TResponse>)methodInfo.Invoke(handler, new[] { request });

            return result;
        }
    }
}