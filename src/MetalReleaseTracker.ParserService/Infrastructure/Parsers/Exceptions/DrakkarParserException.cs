namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class DrakkarParserException : Exception
{
    public DrakkarParserException() : base()
    {
    }

    public DrakkarParserException(string message) : base(message)
    {
    }

    public DrakkarParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}