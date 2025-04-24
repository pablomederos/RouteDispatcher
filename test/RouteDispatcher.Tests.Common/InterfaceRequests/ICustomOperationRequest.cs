using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.InterfaceRequests
{
    // Interface that implements IRequest<T> for testing purposes
    public interface ICustomOperationRequest : IRequest<string>
    {
        string Content { get; }
    }
}
