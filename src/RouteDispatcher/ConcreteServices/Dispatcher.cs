using System;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;
using System.Threading;
using RouteDispatcher.Exceptions;
using System.Linq.Expressions;
using RouteDispatcher.Models;

namespace RouteDispatcher.ConcreteServices
{
#pragma warning disable CS0618 // Type or member is obsolete
    public sealed class Dispatcher : IMediator, IDispatcher
#pragma warning restore CS0618 // Type or member is obsolete
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHandlerCache _handlerCache;
        private readonly DispatcherConfiguration _configurationOptions;

        public Dispatcher(IServiceProvider serviceProvider, DispatcherConfiguration configurationOptions)
        {
            _serviceProvider = serviceProvider;
            _configurationOptions = configurationOptions;
            _handlerCache = (IHandlerCache)_serviceProvider.GetService(typeof(IHandlerCache));
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestType = request.GetType();

            HandlerDelegate<TResponse> handler = _configurationOptions.UseHandlersCache
                ? GetHandlerCompiled<TResponse>(requestType)
                : GetHandler<TResponse>(requestType);

            cancellationToken.ThrowIfCancellationRequested();

            return handler(request, _serviceProvider, cancellationToken);
        }

        private HandlerDelegate<TResponse> GetHandler<TResponse>(Type requestType)
        {
             var handlerType = typeof(IRequestHandler<,>)
                 .MakeGenericType(requestType, typeof(TResponse));
 
             var handler = _serviceProvider
                 .GetService(handlerType)
                 ?? throw new HandlerNotFoundException($"No handler found for request type", requestType);
 
             var methodInfo = handlerType.GetMethod("Handle");
 
             return (
                request,
                serviceProvider,
                cancellationToken
            ) => (Task<TResponse>) methodInfo.Invoke(handler,
                new object[] { request, cancellationToken }
            );
        }
        private HandlerDelegate<TResponse> GetHandlerCompiled<TResponse>(Type requestType)
        {
            var cleanupTimeout = _configurationOptions.DiscardCachedHandlersTimeout;

            CompiledAutocleanDelegate<TResponse> compiled = _handlerCache.GetOrAdd(requestType, requestTypeKey =>
                {
                    var handlerType = typeof(IRequestHandler<,>)
                        .MakeGenericType(requestTypeKey, typeof(TResponse));

                    var compiledExpression = CompileHandlerExpression<TResponse>(requestTypeKey, handlerType);

                    return new CompiledAutocleanDelegate<TResponse>(
                        _handlerCache,
                        requestTypeKey,
                        compiledExpression,
                        cleanupTimeout
                    );
                });

            compiled.Refresh(cleanupTimeout);

            return compiled.Value;
        }
        
        private static HandlerDelegate<TResponse> CompileHandlerExpression<TResponse>(Type requestType, Type handlerType)
        {
            
            var requestParameter = Expression.Parameter(typeof(IRequest<TResponse>), "request");
            var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            
            var getServiceCall = Expression.Call(
                serviceProviderParameter,
                typeof(IServiceProvider).GetMethod("GetService"),
                Expression.Constant(handlerType)
            );
                
            var handlerVariable = Expression.Variable(handlerType, "handler");
            var convertedRequest = Expression.Convert(requestParameter, requestType);
            
            var handleMethod = handlerType.GetMethod("Handle");
            var handleCall = Expression.Call(
                Expression.Convert(handlerVariable, handlerType),
                handleMethod,
                convertedRequest,
                cancellationTokenParameter
            );
                
            var throwExpr = Expression.Throw(
                Expression.New(
                    typeof(HandlerNotFoundException).GetConstructor(new[] { typeof(string) }),
                    Expression.Constant($"No handler found for request type {requestType.Name}")
                ),
                typeof(Task<TResponse>)
            );
                
            var conditionalExpr = Expression.Condition(
                Expression.Equal(handlerVariable, Expression.Constant(null)),
                throwExpr,
                handleCall
            );
                
            var block = Expression.Block(
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
    }}
