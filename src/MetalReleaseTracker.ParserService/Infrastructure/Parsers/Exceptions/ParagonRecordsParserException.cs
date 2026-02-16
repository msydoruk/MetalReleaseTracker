namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class ParagonRecordsParserException : Exception
{
    public ParagonRecordsParserException() : base()
    {
    }

    public ParagonRecordsParserException(string message) : base(message)
    {
    }

    public ParagonRecordsParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
