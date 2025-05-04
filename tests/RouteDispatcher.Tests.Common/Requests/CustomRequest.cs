namespace RouteDispatcher.Tests.Common.Requests
{
    // Concrete request that implements the interface
    public class CustomRequest : ICustomOperationRequest
    {
        public string Content { get; set; } = "Test content";
    }
}
