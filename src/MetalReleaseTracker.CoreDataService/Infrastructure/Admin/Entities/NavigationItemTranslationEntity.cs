using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("NavigationItemTranslations")]
public class NavigationItemTranslationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid NavigationItemId { get; set; }

    [ForeignKey(nameof(NavigationItemId))]
    public NavigationItemEntity NavigationItem { get; set; }

    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; }

    [ForeignKey(nameof(LanguageCode))]
    public LanguageEntity Language { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(160)]
    public string? SeoTitle { get; set; }

    [MaxLength(320)]
    public string? SeoDescription { get; set; }

    [MaxLength(500)]
    public string? SeoKeywords { get; set; }
}
