using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("DistributorTranslations")]
public class DistributorTranslationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DistributorId { get; set; }

    [ForeignKey(nameof(DistributorId))]
    public DistributorEntity Distributor { get; set; }

    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; }

    [ForeignKey(nameof(LanguageCode))]
    public LanguageEntity Language { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
