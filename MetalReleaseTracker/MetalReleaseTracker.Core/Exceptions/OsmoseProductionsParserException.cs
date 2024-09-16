using System;

namespace MetalReleaseTracker.Core.Exceptions
{
    public class OsmoseProductionsParserException : Exception
    {
        public OsmoseProductionsParserException(string message, string url) : base($"{message}. URL: {url}") { }
    }
}
