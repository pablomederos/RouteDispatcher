using RouteDispatcher.Contracts;
using RouteDispatcher.Models;
using RouteDispatcher.Tests.Common.Requests;

namespace RouteDispatcher.Tests.Common.Handlers;

public sealed class TestVoidRequestHandler : IInvocationHandler<TestVoidRequest, Empty>
{
    public Task<Empty> Handle(TestVoidRequest requestWrapper, CancellationToken cancellationToken = default)
    {
        // Mark the request as handled
        requestWrapper.WasHandled = true;
        requestWrapper.Content += "Handled by TestVoidRequestHandler";
        
        return Empty.Value;
    }
}
