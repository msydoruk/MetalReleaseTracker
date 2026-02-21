using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

public class AiVerificationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CatalogueIndexId { get; set; }

    public CatalogueIndexEntity CatalogueIndex { get; set; }

    [Required]
    public string BandName { get; set; }

    [Required]
    public string AlbumTitle { get; set; }

    public bool IsUkrainian { get; set; }

    public double ConfidenceScore { get; set; }

    [Required]
    public string AiAnalysis { get; set; }

    public AiVerificationDecision? AdminDecision { get; set; }

    public DateTime? AdminDecisionDate { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}
