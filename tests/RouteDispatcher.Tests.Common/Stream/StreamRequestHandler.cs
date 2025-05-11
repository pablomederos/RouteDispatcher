using System.Runtime.CompilerServices;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.Tests.Common.Stream
{
    public class StreamRequestHandler : IStreamInvocationHandler<StreamRequest, StreamResponse>
    {
        public static int HandleCallCount { get; private set; }

        public async IAsyncEnumerable<StreamResponse> OnStreamRequested(
            StreamRequest request, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            HandleCallCount++;

            for (int i = 0; i < request.ItemCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (request.ThrowException && i > 0)
                {
                    throw new InvalidOperationException("Test exception from stream handler");
                }

                if (request.DelayMs > 0)
                {
                    await Task.Delay(request.DelayMs, cancellationToken);
                }

                yield return new StreamResponse(i, $"Item {i}");
            }
        }

        public static void ResetCounter()
        {
            HandleCallCount = 0;
        }
    }
}
