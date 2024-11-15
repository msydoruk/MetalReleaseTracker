using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Distributors")]
    public class DistributorEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "The distributor  name is required.")]
        public string Name { get; set; }

        [Required]
        [Url]
        public string ParsingUrl { get; set; }

        [Required]
        [EnumDataType(typeof(DistributorCode))]
        public DistributorCode Code { get; set; }
    }
}
