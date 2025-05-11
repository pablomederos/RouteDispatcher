using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Stream
{
    public class StreamRequest : IStreamRequest<StreamResponse>
    {
        public int ItemCount { get; }
        public bool ThrowException { get; }
        public int DelayMs { get; }

        public StreamRequest(int itemCount, bool throwException = false, int delayMs = 10)
        {
            ItemCount = itemCount;
            ThrowException = throwException;
            DelayMs = delayMs;
        }
    }
}
