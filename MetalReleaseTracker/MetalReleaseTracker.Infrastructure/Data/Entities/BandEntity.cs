using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Bands")]
    public class BandEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "The band name is required.")]
        public string Name { get; set; }
    }
}
