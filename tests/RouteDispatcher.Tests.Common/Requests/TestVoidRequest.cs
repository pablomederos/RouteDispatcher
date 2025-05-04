using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Requests;

/// <summary>
/// Test request class for void-returning Send method tests
/// </summary>
public sealed class TestVoidRequest : IRequest
{
    public string Content { get; set; } = string.Empty;
    public bool WasHandled { get; set; }
}
