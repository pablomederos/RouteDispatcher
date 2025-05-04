using System.Threading.Tasks;

namespace RouteDispatcher.Models;

public record Empty
{
    private Empty() { }
    public static readonly Empty Value = new ();
    
    public static implicit operator Task<Empty>(Empty _) => Task.FromResult(Value);
}