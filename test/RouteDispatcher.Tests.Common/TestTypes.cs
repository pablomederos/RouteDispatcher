using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common
{
    public class TestRequest : IRequest<string> { }
    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken _)
        {
            return Task.FromResult("Test Response");
        }
    }

    public interface IOtherRequest : IRequest<string> { }
    public class OtherRequest : IOtherRequest { }
}