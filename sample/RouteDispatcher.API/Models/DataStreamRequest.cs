using RouteDispatcher.Contracts;

namespace RouteDispatcher.API.Models;

public class DataStreamRequest : IStreamRequest<DataStreamResponse>
{
    public int NumberOfItems { get; set; }
    public string? Filter { get; set; }

    public DataStreamRequest(int numberOfItems, string? filter = null)
    {
        NumberOfItems = numberOfItems;
        Filter = filter;
    }
    
    // Parameter-less constructor for model binding
    public DataStreamRequest() 
    {
        NumberOfItems = 10; // Default value
    }
}
