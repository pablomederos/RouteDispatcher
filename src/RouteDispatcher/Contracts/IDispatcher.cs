using System.Threading.Tasks;
using System.Threading;

namespace RouteDispatcher.Contracts
{
    public interface IDispatcher
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        Task Broadcast<TMessage>(TMessage request, CancellationToken cancellationToken = default)
            where TMessage : class;
    }
}