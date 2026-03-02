using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class ParsingRunEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public ParsingJobType JobType { get; set; }

    [Required]
    public DistributorCode DistributorCode { get; set; }

    [Required]
    public ParsingRunStatus Status { get; set; }

    public int TotalItems { get; set; }

    public int ProcessedItems { get; set; }

    public int FailedItems { get; set; }

    [Required]
    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
