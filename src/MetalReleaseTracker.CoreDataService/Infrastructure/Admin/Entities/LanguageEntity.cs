using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("Languages")]
public class LanguageEntity
{
    [Key]
    [MaxLength(5)]
    public string Code { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string NativeName { get; set; }

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsDefault { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}
