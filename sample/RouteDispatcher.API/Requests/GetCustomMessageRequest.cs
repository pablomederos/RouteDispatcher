namespace RouteDispatcher.API.Requests
{
    public sealed class GetCustomMessageRequest : IMessageOperation
    {
        public string Content { get; init; } = string.Empty;
    }
}
