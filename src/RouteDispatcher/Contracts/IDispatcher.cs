using System.Threading.Tasks;
using System.Threading;

namespace RouteDispatcher.Contracts
{
    public interface IDispatcher
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}