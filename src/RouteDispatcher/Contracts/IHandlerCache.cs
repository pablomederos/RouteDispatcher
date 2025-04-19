using System;
using System.Threading;
using System.Threading.Tasks;

namespace RouteDispatcher.Contracts
{
    public interface IHandlerCache
    {
        CompiledHandlerCaller<TResponse> GetOrAdd<TResponse>(Type requestType, Func<Type, CompiledHandlerCaller<TResponse>> value);
    }

    public delegate Task<TResponse> CompiledHandlerCaller<TResponse>(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}