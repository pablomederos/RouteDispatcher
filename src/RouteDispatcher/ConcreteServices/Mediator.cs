using System;
using System.Threading.Tasks;
using RouteDispatcher.Contracts;
using System.Threading;
using RouteDispatcher.Exceptions;
using System.Linq.Expressions;

namespace RouteDispatcher.ConcreteServices
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHandlerCache _handlerCache;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _handlerCache = (IHandlerCache) _serviceProvider.GetService(typeof(IHandlerCache));
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestType = request.GetType();

            CompiledHandlerCaller<TResponse> handler = GetHandler<TResponse>(requestType);

            cancellationToken.ThrowIfCancellationRequested();

            return handler(request, _serviceProvider, cancellationToken);
        }
        
        private CompiledHandlerCaller<TResponse> GetHandler<TResponse>(Type requestType)
            => _handlerCache.GetOrAdd(requestType, requestTypeKey =>
                {
                    var handlerType = typeof(IRequestHandler<,>)
                        .MakeGenericType(requestTypeKey, typeof(TResponse));

                    return CompileHandlerExpression<TResponse>(requestTypeKey, handlerType);
                });
        
        private static CompiledHandlerCaller<TResponse> CompileHandlerExpression<TResponse>(Type requestType, Type handlerType)
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
                .Lambda<CompiledHandlerCaller<TResponse>>(
                block,
                requestParameter,
                serviceProviderParameter,
                cancellationTokenParameter
            );
                
            return functionExpression.Compile();
        }
    }}
