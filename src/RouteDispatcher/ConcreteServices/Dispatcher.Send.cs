using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;
using RouteDispatcher.Exceptions;
using RouteDispatcher.Models;

namespace RouteDispatcher.ConcreteServices;

public sealed partial class Dispatcher
{
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
        => Send<Empty>(request, cancellationToken);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Type requestType = request.GetType();

        HandlerDelegate<TResponse> handler = _configurationOptions.UseHandlersCache
            ? GetHandlerCompiled<TResponse>(requestType)
            : GetHandler<TResponse>(requestType);

        cancellationToken.ThrowIfCancellationRequested();

        return handler(request, _serviceProvider, cancellationToken);
    }

    private HandlerDelegate<TResponse> GetHandler<TResponse>(Type requestType)
    {
        Type handlerType = InvocationHandlerType
            .MakeGenericType(requestType, typeof(TResponse));

        object? handler = _serviceProvider
            .GetService(handlerType);

        // Todo: Remove with obsolete code
        if (handler is null)
        {
            Type obsoleteHandlerType = RequestHandlerType
                .MakeGenericType(requestType, typeof(TResponse));

            handler = _serviceProvider
                .GetService(obsoleteHandlerType);
        }
        
        if (handler is null)
            throw new HandlerNotFoundException("No handler found for request type", requestType);

        MethodInfo methodInfo = handlerType
            .GetMethod(
                HandlerMethodName
            )!;
 
        return (
            request,
            _,
            cancellationToken
        ) => (Task<TResponse>) methodInfo.Invoke(
            handler!,
            new object[]
            {
                request, 
                cancellationToken
            }
        )!;
    }
    private HandlerDelegate<TResponse> GetHandlerCompiled<TResponse>(Type requestType)
    {
        if(!_configurationOptions.UseHandlersCache)
            throw new InvalidOperationException("Handlers cache is disabled.");
        
        bool keepCacheForEver = _configurationOptions.KeepCacheForEver;
        TimeSpan cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

        CompiledAutocleanDelegate<TResponse> compiled = _handlerCache
            !.GetOrAdd(requestType, requestTypeKey =>
            {
                Type handlerType = InvocationHandlerType
                    .MakeGenericType(requestTypeKey, typeof(TResponse));

                var compiledExpression = CompileHandlerExpression<TResponse>(requestTypeKey, handlerType);

                return new CompiledAutocleanDelegate<TResponse>(
                    _handlerCache,
                    requestTypeKey,
                    compiledExpression,
                    cleanupTimeout,
                    keepCacheForEver
                );
            });

        if (!keepCacheForEver)
            compiled.Refresh(cleanupTimeout);

        return compiled.Value;
    }
        
    private static HandlerDelegate<TResponse> CompileHandlerExpression<TResponse>(Type requestType, Type handlerType)
    {
            
        ParameterExpression requestParameter = Expression
            .Parameter(typeof(IRequest<TResponse>), "request");
        ParameterExpression serviceProviderParameter = Expression
            .Parameter(typeof(IServiceProvider), "serviceProvider");
        ParameterExpression cancellationTokenParameter = Expression
            .Parameter(typeof(CancellationToken), "cancellationToken");
            
        MethodCallExpression getServiceCall = Expression.Call(
            serviceProviderParameter,
            typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService))!,
            Expression.Constant(handlerType)
        );
                
        ParameterExpression handlerVariable = Expression.Variable(handlerType, "handler");
        UnaryExpression convertedRequest = Expression.Convert(requestParameter, requestType);
            
        MethodInfo handleMethod = handlerType
            .GetMethod(
                HandlerMethodName
            ) ?? throw new HandlerNotFoundException($"No handler found for request type {requestType.Name}");
            
        MethodCallExpression handleCall = Expression.Call(
            Expression.Convert(handlerVariable, handlerType),
            handleMethod,
            convertedRequest,
            cancellationTokenParameter
        );
                
        UnaryExpression throwExpr = Expression.Throw(
            Expression.New(
                typeof(HandlerNotFoundException).GetConstructor(new[] { typeof(string) })!,
                Expression.Constant($"No handler found for request type {requestType.Name}")
            ),
            typeof(Task<TResponse>)
        );
                
        ConditionalExpression conditionalExpr = Expression.Condition(
            Expression.Equal(handlerVariable, Expression.Constant(null)),
            throwExpr,
            handleCall
        );
                
        BlockExpression block = Expression.Block(
            new[] { handlerVariable },
            Expression.Assign(handlerVariable, Expression.Convert(getServiceCall, handlerType)),
            conditionalExpr);

        var functionExpression = Expression
            .Lambda<HandlerDelegate<TResponse>>(
                block,
                requestParameter,
                serviceProviderParameter,
                cancellationTokenParameter
            );
                
        return functionExpression.Compile();
    }
}