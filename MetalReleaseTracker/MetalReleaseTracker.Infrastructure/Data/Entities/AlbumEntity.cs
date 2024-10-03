using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Albums")]
    public class AlbumEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DistributorId { get; set; }

        [ForeignKey("DistributorId")]
        public DistributorEntity Distributor { get; set; }

        [Required]
        public Guid BandId { get; set; }

        [ForeignKey("BandId")]
        public BandEntity Band { get; set; }

        [Required]
        [RegularExpression("^[A-Z0-9-]*$", ErrorMessage = "SKU can only contain uppercase letters, numbers and hyphens.")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "The album name is required.")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        public string Genre { get; set; }

        [Range(1, float.MaxValue, ErrorMessage = "Min price is 1$")]
        public float Price { get; set; }

        [Url]
        public string PurchaseUrl { get; set; }

        [Url]
        public string PhotoUrl { get; set; }

        [EnumDataType(typeof(MediaType))]
        public MediaType Media { get; set; }

        public string Label { get; set; }

        public string Press { get; set; }

        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must contain at least 10 characters.")]
        public string Description { get; set; }

        public DateTime ModificationTime { get; set; }

        [EnumDataType(typeof(AlbumStatus))]
        public AlbumStatus Status { get; set; }
    }
}
