using System;

namespace RouteDispatcher.Contracts
{
    [Obsolete("Use IInvocationHandler interface instead.", error: false)]
    public interface IRequestHandler<in TRequest, TResponse> : IInvocationHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {}
}