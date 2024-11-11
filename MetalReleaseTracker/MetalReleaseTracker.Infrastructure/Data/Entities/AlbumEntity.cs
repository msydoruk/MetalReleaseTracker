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
        public string SKU { get; set; }

        [Required(ErrorMessage = "The album name is required.")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        public string? Genre { get; set; }

        public float Price { get; set; }

        [Url]
        public string PurchaseUrl { get; set; }

        [Url]
        public string PhotoUrl { get; set; }

        [EnumDataType(typeof(MediaType))]
        public MediaType Media { get; set; }

        public string Label { get; set; }

        public string Press { get; set; }

        public string Description { get; set; }

        public DateTime ModificationTime { get; set; }

        [EnumDataType(typeof(AlbumStatus))]
        public AlbumStatus Status { get; set; }
    }
}
