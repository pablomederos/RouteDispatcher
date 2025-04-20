using System;
using System.Threading;
using System.Threading.Tasks;

namespace RouteDispatcher.Contracts
{
    /// <summary>
    /// <remarks>
    /// This interface is obsolete. Use <see cref="IDispatcher"/> instead.
    /// </remarks>
    /// </summary>
    [Obsolete("Use IDispatcher interface instead.", error: false)]
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}