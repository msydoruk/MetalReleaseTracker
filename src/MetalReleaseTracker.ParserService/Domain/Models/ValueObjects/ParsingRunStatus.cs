namespace MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

public enum ParsingRunStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}
