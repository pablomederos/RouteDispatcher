using System.Threading;
using System.Threading.Tasks;

namespace RouteDispatcher.Contracts;

public interface IMessageHandler<in TMessage>
    where TMessage : IMessage
{
    Task OnMessage(TMessage message, CancellationToken cancellationToken);
}