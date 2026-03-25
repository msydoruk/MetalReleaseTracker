using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("Distributors")]
public class DistributorEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The distributor name is required.")]
    public string Name { get; set; }

    public DistributorCode Code { get; set; }

    public bool IsVisible { get; set; } = true;

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionUa { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(10)]
    public string? CountryFlag { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }
}
