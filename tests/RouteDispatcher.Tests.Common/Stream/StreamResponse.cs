using System;

namespace RouteDispatcher.Tests.Common.Stream
{
    public class StreamResponse
    {
        public int Index { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }

        public StreamResponse(int index, string value)
        {
            Index = index;
            Value = value;
            Timestamp = DateTime.UtcNow;
        }
    }
}
