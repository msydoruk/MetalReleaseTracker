namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class SeasonOfMistParserException : Exception
{
    public SeasonOfMistParserException() : base()
    {
    }

    public SeasonOfMistParserException(string message) : base(message)
    {
    }

    public SeasonOfMistParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
