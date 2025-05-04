using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Handlers;

#pragma warning disable CS0618 // Type or member is obsolete
public class TestRequestHandler : IRequestHandler<TestRequest, string>
#pragma warning restore CS0618 // Type or member is obsolete
{
    public Task<string> Handle(TestRequest request, CancellationToken _)
    {
        return Task.FromResult("Test Response");
    }
}