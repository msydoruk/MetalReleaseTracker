using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("Translations")]
public class TranslationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Key { get; set; }

    [Required]
    [MaxLength(5)]
    public string Language { get; set; }

    [Required]
    public string Value { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
