using System;

namespace MetalReleaseTracker.Infrastructure.Exceptions
{
    public class OsmoseProductionsParserException : Exception
    {
        public OsmoseProductionsParserException() : base() { }

        public OsmoseProductionsParserException(string message) : base(message) { }

        public OsmoseProductionsParserException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}