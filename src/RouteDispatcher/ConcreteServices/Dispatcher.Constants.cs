using System;
using RouteDispatcher.Contracts;

namespace RouteDispatcher.ConcreteServices;

public partial class Dispatcher
{
#pragma warning disable CS0618 // Type or member is obsolete
    public static readonly Type RequestHandlerType = typeof(IRequestHandler<,>);
#pragma warning restore CS0618 // Type or member is obsolete
    
    public static readonly Type InvocationHandlerType = typeof(IInvocationHandler<,>);
    public static readonly Type StreamInvocationHandlerType = typeof(IStreamInvocationHandler<,>);
    public static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
    public const string HandlerMethodName = nameof(IInvocationHandler<IRequest<object>, object>.Handle);
    public const string StreamMethodName = nameof(IStreamInvocationHandler<IStreamRequest<object>, object>.OnStreamRequested);
    public const string MessageMethodName = nameof(IMessageHandler<IMessage>.OnMessage);
}