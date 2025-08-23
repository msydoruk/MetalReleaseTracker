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
}
