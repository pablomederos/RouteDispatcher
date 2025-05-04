using RouteDispatcher.Models;

namespace RouteDispatcher.Contracts;

public interface IRequest : IRequest<Empty>
{}

public interface IRequest<out TResponse>
{
}