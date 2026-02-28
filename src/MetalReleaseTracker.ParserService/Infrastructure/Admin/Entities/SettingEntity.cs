using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

public class SettingEntity
{
    [Key]
    [MaxLength(200)]
    public string Key { get; set; }

    [Required]
    public string Value { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
