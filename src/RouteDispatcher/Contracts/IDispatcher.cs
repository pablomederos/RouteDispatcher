using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Models;

namespace RouteDispatcher.Contracts
{
    public interface IDispatcher
    {
        

        /// <summary>
        /// Sends a request to a single handler and doesn't return a result.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="request">The request to send implementing <see cref="IRequest"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is designed for scenarios where you want to ensure that only one handler processes the request.
        /// If you don't want to return <see cref="Empty"/> and don't care about multiple handlers to process the request,
        /// consider using the <c>Broadcast</c> method instead, which is specifically designed for sending messages to
        /// multiple handlers simultaneously.
        /// </remarks>
        Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest;
        
        
        /// <summary>
        /// Sends a request to a single handler and returns a result of type <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to send, which must implement <see cref="IRequest{TResponse}"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with the result of type <typeparamref name="TResponse"/>.</returns>
        /// <remarks>
        /// This method sends the request to exactly one handler and expects a response of type <typeparamref name="TResponse"/>.
        /// The handler must implement <see cref="IInvocationHandler{TRequest, TResponse}"/> for the specific request and response types.
        /// </remarks>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        IAsyncEnumerable<TResponse> Stream<TResponse>(IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default);
        Task Broadcast<TMessage>(TMessage request, CancellationToken cancellationToken = default)
            where TMessage : class;
    }
}