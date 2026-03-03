using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class ParsingRunItemEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ParsingRunId { get; set; }

    [Required]
    public string ItemDescription { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public string[] Categories { get; set; } = [];

    [Required]
    public DateTime ProcessedAt { get; set; }

    public ParsingRunEntity ParsingRun { get; set; } = null!;
}
