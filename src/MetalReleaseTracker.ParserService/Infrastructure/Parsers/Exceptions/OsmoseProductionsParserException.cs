namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class OsmoseProductionsParserException : Exception
{
    public OsmoseProductionsParserException() : base()
    {
    }

    public OsmoseProductionsParserException(string message) : base(message)
    {
    }

    public OsmoseProductionsParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}