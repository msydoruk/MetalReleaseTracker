namespace MetalReleaseTracker.CoreDataService.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Bands")]
public class BandEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The band name is required.")]
    public string Name { get; set; }
}
