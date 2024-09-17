using System;

namespace MetalReleaseTracker.Core.Exceptions
{
    public class OsmoseProductionsParserException : Exception
    {
        public OsmoseProductionsParserException(string message) : base($"{message}") { }
    }
}
