using System;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;
using System.Threading;

namespace RouteDispatcher.ConcreteServices
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if(cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResponse>(cancellationToken);

            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(requestType, typeof(TResponse));

            var handler = _serviceProvider
                .GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for request type {requestType.Name}");

            var methodInfo = handlerType.GetMethod("Handle");

            if(cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResponse>(cancellationToken);
            
            return (Task<TResponse>)methodInfo.Invoke(handler, new object[] { request, cancellationToken });
        }
    }
}