Author: Pablo Gabriel Mederos (La Cueva del Insecto)

# RouteDispatcher [![NuGet](https://img.shields.io/nuget/v/RouteDispatcher.svg)](https://www.nuget.org/packages/RouteDispatcher/)

A simple mediator implementation for .NET. The package is available on NuGet Gallery.

## Description

This library provides a simple mediator implementation that allows you to decouple request handling from the request source. It uses the `IServiceProvider` to resolve the appropriate handler for a given request.

## Usage

1.  Install the NuGet package:

    ```bash
    dotnet add package RouteDispatcher
    ```

2.  Create a request interface:

    ```csharp
    public interface IMyRequest : IRequest<MyResponse> { }
    ```

3.  Create a request class:

    ```csharp
    public class MyRequest : IMyRequest { }
    ```

4.  Create a response class:

    ```csharp
    public class MyResponse { }
    ```

5.  Create a request handler:

    ```csharp
    public class MyRequestHandler : IRequestHandler<MyRequest, MyResponse>
    {
        public Task<MyResponse> Handle(MyRequest request)
        {
            // Handle the request and return a response
            return Task.FromResult(new MyResponse());
        }
    }
    ```

6.  Resolve the `IMediator` from the `IServiceProvider` or through dependency injection:

    ```csharp
    // From IServiceProvider
    var mediator = serviceProvider.GetRequiredService<IMediator>();

    // Or through dependency injection
    public class MyClass
    {
        private readonly IMediator _mediator;

        public MyClass(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
    ```

8.  Send the request to the mediator:

    ```csharp
    var response = await mediator.Send(new MyRequest());
    ```

## Extension Method Usage

It is **required** to use the `AddRouteDispatcher` extension method to register all `IRequestHandler` implementations. This method accepts a `params Assembly[]` array, so you can call it with one or more assemblies:

```csharp
// If handlers are in the same assembly, you can call it without arguments:
services.AddRouteDispatcher();

// Otherwise, specify one or more assemblies:
services.AddRouteDispatcher(typeof(Program).Assembly);
// Or
services.AddRouteDispatcher(typeof(AnotherAssemblyWithRequestHandlers).Assembly, typeof(YetAnotherAssemblyWithRequestHandlers).Assembly);
```

## License

MIT