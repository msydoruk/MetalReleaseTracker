using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("NewsArticles")]
public class NewsArticleEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ChipLabel { get; set; }

    [Required]
    [MaxLength(20)]
    public string ChipColor { get; set; }

    [Required]
    [MaxLength(100)]
    public string IconName { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<NewsArticleTranslationEntity> Translations { get; set; } = [];
}
