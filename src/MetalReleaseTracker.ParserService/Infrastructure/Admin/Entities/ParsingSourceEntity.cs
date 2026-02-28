using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

public class ParsingSourceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DistributorCode DistributorCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(2000)]
    public string ParsingUrl { get; set; }

    public bool IsEnabled { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
