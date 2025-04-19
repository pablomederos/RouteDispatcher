using System;

namespace RouteDispatcher.Exceptions
{
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(string message) : base(message)
        {
        }

        public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public HandlerNotFoundException(string message, Type handlerType) : base(message)
        {
            HandlerType = handlerType;
        }
        public HandlerNotFoundException(string message, Type handlerType, Exception innerException) : base(message, innerException)
        {
            HandlerType = handlerType;
        }

        
        public Type HandlerType { get; }
        public override string Message => base.Message + (HandlerType != null ? $" Handler Type: {HandlerType.Name}" : string.Empty);
        
        public override string ToString()
        {
            return $"{base.ToString()}, Handler Type: {HandlerType?.Name}";
        }
    }
}