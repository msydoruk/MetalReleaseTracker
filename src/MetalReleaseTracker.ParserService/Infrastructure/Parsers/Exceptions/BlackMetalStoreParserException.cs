namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class BlackMetalStoreParserException : Exception
{
    public BlackMetalStoreParserException() : base()
    {
    }

    public BlackMetalStoreParserException(string message) : base(message)
    {
    }

    public BlackMetalStoreParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
