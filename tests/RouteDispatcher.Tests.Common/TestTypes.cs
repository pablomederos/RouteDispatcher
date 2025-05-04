using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common
{
    public class TestRequest : IRequest<string> { }

    public interface IOtherRequest : IRequest<string> { }
    public class OtherRequest : IOtherRequest { }
}