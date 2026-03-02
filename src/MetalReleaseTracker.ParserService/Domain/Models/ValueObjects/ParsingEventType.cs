namespace MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

public enum ParsingEventType
{
    Started = 0,
    Progress = 1,
    Error = 2,
    Completed = 3
}
