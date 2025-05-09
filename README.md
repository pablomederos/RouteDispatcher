

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