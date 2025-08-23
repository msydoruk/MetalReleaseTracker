namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class DarkerThanBlackRecordsParserExeption : Exception
{
    public DarkerThanBlackRecordsParserExeption() : base()
    {
    }

    public DarkerThanBlackRecordsParserExeption(string message) : base(message)
    {
    }

    public DarkerThanBlackRecordsParserExeption(string message, Exception innerException) : base(message, innerException)
    {
    }
}