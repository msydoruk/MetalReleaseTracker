using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("NavigationItems")]
public class NavigationItemEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleUa { get; set; }

    [Required]
    [MaxLength(200)]
    public string Path { get; set; }

    [Required]
    [MaxLength(100)]
    public string IconName { get; set; }

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    public bool IsProtected { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedAt { get; set; }
}
