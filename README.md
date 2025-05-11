

# RouteDispatcher [![NuGet](https://img.shields.io/nuget/v/RouteDispatcher.svg)](https://www.nuget.org/packages/RouteDispatcher/)
[![Production Workflow](https://github.com/pablomederos/RouteDispatcher/actions/workflows/main.yaml/badge.svg)](https://github.com/pablomederos/RouteDispatcher/actions/workflows/main.yaml)    
[![Develop Test Workflow](https://github.com/pablomederos/RouteDispatcher/actions/workflows/develop.yaml/badge.svg)](https://github.com/pablomederos/RouteDispatcher/actions/workflows/develop.yaml)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/pablomederos/RouteDispatcher)

A simple mediator implementation for .NET

## Description

This library provides a simple mediator implementation that allows you to decouple request handling from the request source. It uses the `IServiceProvider` to resolve the appropriate handler for a given request.

## Usage

1. Install the NuGet package:

    ```bash    
    dotnet add package RouteDispatcher
    ``` 

2. Create a request class:

    ```csharp    
    public class MyRequest : IRequest<MyResponse> { }    
    ```  
3. Create a response class:

    ```csharp    
    public class MyResponse { }    
	``` 

4. Create a request handler:

   ```csharp  
   public class MyRequestHandler : IInvocationHandler<MyRequest, MyResponse>   
   {   
      public Task<MyResponse> Handle(MyRequest request) {   
          // Handle the request and return a response   
          return Task.FromResult(new MyResponse());   
      }   
   }  
   ```

5. Resolve the `IDispatcher` from the `IServiceProvider` or through dependency injection:

    ```csharp    
    // From IServiceProvider    
     var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();    
     // Or through dependency injection 
     public class MyClass { 
	     private readonly IDispatcher _dispatcher;    
	     public MyClass(IDispatcher dispatcher) { 
		     _dispatcher = dispatcher; 
	     } 
     }   
	``` 

6. Send the request to the mediator:

    ```csharp    
	    var response = await _dispatcher.Send(new MyRequest());    
	``` 

## Request without response (void)

If you don't need a response from your request handler, you can use `IRequest` without a generic parameter:

1. Create a request class:

   ```csharp    
   public class MyVoidRequest : IRequest     
   { 
       public string Content { get; set; } 
       public bool WasHandled { get; set; } 
   } 
   ```  
2. Create a request handler:

   ```csharp
   public class MyVoidRequestHandler : IInvocationHandler<MyVoidRequest, Empty> 
   { 
       public Task<Empty> Handle(MyVoidRequest request, CancellationToken cancellationToken) 
       { 
           // Handle the request without returning a response
           request.Content += "Handled by MyVoidRequestHandler";
           request.WasHandled = true; 
           return Empty.Value; 
       } 
   }  
   ``` 

3. Send the request using the `IDispatcher`:

    ```csharp    
    await _dispatcher.Send(
	    new MyVoidRequest { 
		    Content = "Initial content: " 
	    }
    );
	```    

## Message Handling

For scenarios where you want to implement the publish-subscribe pattern, you can use `IMessage` and `IMessageHandler<T>`:

1. Create a message class:

   ```csharp    
   public class MyMessage : IMessage    
   { 
       public string Content { get; set; } 
   } 
   ```    
2. Create a message handler:

   ```csharp    
   public class MyMessageHandler : IMessageHandler<MyMessage>    
   { 
       public Task OnMessage(MyMessage message, CancellationToken cancellationToken) 
       { 
           // Process the message 
           Console.WriteLine($"Received message: {message.Content}"); 
           return Task.CompletedTask; 
       } 
   }   
   ``` 

3. Publish a message using the `IDispatcher`:

    ```csharp    
    await _dispatcher.Broadcast(
	    new MyMessage { 
		    Content = "Hi from message " 
	    }
    );
	```   
   
## Stream Handling

For scenarios where you need to stream multiple responses for a single request, `RouteDispatcher` supports this through `IStreamRequest<TResponse>` and handlers that return `IAsyncEnumerable<TResponse>`. This is useful for operations that produce data over time, such as processing large datasets or long-running tasks that provide incremental updates.

1.  Define your stream request, which must implement `IStreamRequest<TResponse>` where `TResponse` is the type of item in the stream:

    ```csharp
    // Request for a stream of MyStreamItemResponse objects
    public class MyDataStreamRequest : IStreamRequest<MyStreamItemResponse>
    {
        public int NumberOfItemsToGenerate { get; set; }
    }
    ```

2.  Define the class for the items that will be part of the stream:

    ```csharp
    public class MyStreamItemResponse
    {
        public int ItemId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
    ```

3.  Create a handler for your stream request. This handler should implement an interface like `IStreamInvocationHandler<TRequest, TResponse>` (where `TRequest` is your stream request type and `TResponse` is your stream item response type). The method responsible for handling the stream (e.g., `Handle`) must return an `IAsyncEnumerable<TResponse>`:

    ```csharp
    // Handler for MyDataStreamRequest, streaming MyStreamItemResponse objects
    public class MyDataStreamRequestHandler : IStreamInvocationHandler<MyDataStreamRequest, MyStreamItemResponse>
    {
        // The Handle method (or the specific method name configured in your Dispatcher for streams)
        // now returns an IAsyncEnumerable<MyStreamItemResponse>
        public async IAsyncEnumerable<MyStreamItemResponse> Handle(
            MyDataStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken) // [EnumeratorCancellation] is recommended
        {
            for (int i = 0; i < request.NumberOfItemsToGenerate; i++)
            {
                // Ensure operation is cancelled if requested by the consumer
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate asynchronous work to generate an item
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

                yield return new MyStreamItemResponse
                {
                    ItemId = i + 1,
                    Message = $"This is item {i + 1} from the stream.",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
    ```
    *Note: The `[EnumeratorCancellation]` attribute on the `CancellationToken` parameter is good practice for `async IAsyncEnumerable` methods.*

4.  Use the `Stream` method on the `IDispatcher` to initiate the stream. You can then consume the `IAsyncEnumerable<TResponse>` using `await foreach`:

    ```csharp
    // Resolve the IDispatcher from IServiceProvider or through dependency injection
    // var dispatcher = serviceProvider.GetRequiredService<IDispatcher>(); 
    // Assuming _dispatcher is an injected IDispatcher instance:

    var streamRequest = new MyDataStreamRequest { NumberOfItemsToGenerate = 5 };
    try
    {
        await foreach (var item in _dispatcher.Stream(streamRequest, CancellationToken.None))
        {
            Console.WriteLine($"Received: ID={item.ItemId}, Msg='{item.Message}', Time='{item.Timestamp:O}'");
            // Process each item as it arrives
            // Example: if (item.ItemId == 3) break; // to stop consumption early
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Stream consumption was cancelled.");
    }
    finally
    {
         Console.WriteLine("Finished consuming the stream or cancellation occurred.");
    }
    ```

This approach allows the client to process each item as it's generated and yielded by the handler, without needing to wait for the entire sequence of responses to be collected first. This is highly efficient for handling large or continuous data flows.

## Extension Method Usage

It is **required** to use the `AddRouteDispatcher` extension method to register all `IInvocationHandler<,>` and `IMessageHandler<>` implementations. This method accepts a `params Assembly[]` array, so you can call it with one or more assemblies:

```csharp
 // If handlers are in the same assembly, you can call it without arguments: 
 services.AddRouteDispatcher();    
 // Otherwise, specify one or more assemblies:
 services.AddRouteDispatcher(typeof(Program).Assembly); 
 // Or
 services.AddRouteDispatcher(
     typeof(AnotherAssemblyWithRequestHandlers).Assembly,
     typeof(YetAnotherAssemblyWithRequestHandlers).Assembly
 ); 
```
## Recomended ⚠️
You can also configure the dispatcher with caching using the `AddRouteDispatcher` extension method with an `Action<DispatcherConfiguration>`. The cache is used to avoid the use of reflection when obtaining the handler type again and again, and reuse a type previously discovered:

```csharp
services.AddRouteDispatcher(options => {    
     // options.Assemblies: If not set, the current assembly will be used. 
     options.Assemblies = new[] { typeof(Program).Assembly }; 
     // options.UseHandlersCache: If false, the following configurations are not needed. 
     options.UseHandlersCache = true; 
     // options.DiscardCachedHandlersTimeout: Allows the GC to clean up memory from handlers that are not used for a long time.
     options.DiscardCachedHandlersTimeout = TimeSpan.FromSeconds(30);
     // default to TimeSpan.FromSeconds(30) 
     // options.KeepCacheForEver: Prevents the deletion of elements from the cache. 
     options.KeepCacheForEver = false; // Default to false
 }); 
```   
## License

MIT