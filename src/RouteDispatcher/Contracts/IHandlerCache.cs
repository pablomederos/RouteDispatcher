using System;
using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Models;

namespace RouteDispatcher.Contracts
{
    public interface IHandlerCache
    {
        CompiledAutocleanDelegate<TResponse> GetOrAdd<TResponse>(Type requestType, Func<Type, CompiledAutocleanDelegate<TResponse>> value);
        void TryRemove(Type item);
    }

    public delegate Task<TResponse> HandlerDelegate<TResponse>(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}