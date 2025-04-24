namespace RouteDispatcher.Tests.Common.InterfaceRequests
{
    // Concrete request that implements the interface
    public class CustomRequest : ICustomOperationRequest
    {
        public string Content { get; set; } = "Test content";
    }
}
