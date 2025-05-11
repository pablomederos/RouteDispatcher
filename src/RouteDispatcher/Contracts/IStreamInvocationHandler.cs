using System.Collections.Generic;
using System.Threading;

namespace RouteDispatcher.Contracts
{
    public interface IStreamInvocationHandler<in TRequest, out TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        IAsyncEnumerable<TResponse> OnStreamRequested(TRequest request, CancellationToken cancellationToken = default);
    }
}