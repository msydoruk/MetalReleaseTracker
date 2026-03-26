using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("NewsArticleTranslations")]
public class NewsArticleTranslationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid NewsArticleId { get; set; }

    [ForeignKey(nameof(NewsArticleId))]
    public NewsArticleEntity NewsArticle { get; set; }

    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; }

    [ForeignKey(nameof(LanguageCode))]
    public LanguageEntity Language { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    [MaxLength(160)]
    public string? SeoTitle { get; set; }

    [MaxLength(320)]
    public string? SeoDescription { get; set; }

    [MaxLength(500)]
    public string? SeoKeywords { get; set; }
}
