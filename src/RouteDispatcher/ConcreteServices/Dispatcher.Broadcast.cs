using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RouteDispatcher.ConcreteServices
{
    public sealed partial class Dispatcher
    {
        public async Task Broadcast<TMessage>(TMessage request, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if(request is null)
                throw new ArgumentNullException(nameof(request));
        
            Type notificationType = request.GetType();
            Type handlerType = MessageHandlerType
                .MakeGenericType(notificationType);
        
            var handlers = _serviceProvider
                .GetServices(handlerType)
                .ToArray();
        
            if (!handlers.Any())
                throw new InvalidOperationException($"No broadcast handlers found for type [{notificationType.Name}].");
        
            var exceptions = new List<Exception>();
            
            foreach (var handler in handlers)
            {
                MethodInfo? methodInfo = handlerType.GetMethod(MessageMethodName);
                
                try
                {
                    Task task = methodInfo
                        !.Invoke(
                            handler, 
                            new object[]
                            {
                                request, cancellationToken
                            }
                        ) as Task 
                        ?? throw new InvalidOperationException($"Message handler method [{MessageMethodName}] cannot return null.");
                    
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if(_configurationOptions.IgnoreBroadcastFailuresUntilTheEnd)
                        exceptions.Add(ex);
                    else
                        throw new AggregateException(ex);
                }
            }
            
            if(exceptions.Count > 0) 
                throw new AggregateException(exceptions);
        }
    }
}
