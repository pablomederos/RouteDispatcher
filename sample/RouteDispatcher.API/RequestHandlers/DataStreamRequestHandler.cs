using System.Runtime.CompilerServices;
using RouteDispatcher.API.Models;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.RequestHandlers;

public class DataStreamRequestHandler : IStreamInvocationHandler<DataStreamRequest, DataStreamResponse>
{
    public async IAsyncEnumerable<DataStreamResponse> OnStreamRequested(
        DataStreamRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.NumberOfItems; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate some work or data fetching
            await Task.Delay(100, cancellationToken);

            var responseItem = new DataStreamResponse
            {
                ItemId = i + 1,
                Message = $"Streaming item {i + 1}. Filter: {request.Filter ?? "N/A"}",
                Timestamp = DateTime.UtcNow
            };
            
            yield return responseItem;
        }
    }
}
