using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingRunDto
{
    public Guid Id { get; set; }

    public ParsingJobType JobType { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public ParsingRunStatus Status { get; set; }

    public int TotalItems { get; set; }

    public int ProcessedItems { get; set; }

    public int FailedItems { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
