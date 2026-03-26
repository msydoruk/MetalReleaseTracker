using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("BandTranslations")]
public class BandTranslationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BandId { get; set; }

    [ForeignKey(nameof(BandId))]
    public BandEntity Band { get; set; }

    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; }

    [ForeignKey(nameof(LanguageCode))]
    public LanguageEntity Language { get; set; }

    public string? Description { get; set; }

    [MaxLength(160)]
    public string? SeoTitle { get; set; }

    [MaxLength(320)]
    public string? SeoDescription { get; set; }

    [MaxLength(500)]
    public string? SeoKeywords { get; set; }
}
