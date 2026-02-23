using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

public class AiAgentEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string SystemPrompt { get; set; }

    [Required]
    [MaxLength(100)]
    public string Model { get; set; }

    public int MaxTokens { get; set; }

    public int MaxConcurrentRequests { get; set; }

    public int DelayBetweenBatchesMs { get; set; }

    [Required]
    public string ApiKey { get; set; }

    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
