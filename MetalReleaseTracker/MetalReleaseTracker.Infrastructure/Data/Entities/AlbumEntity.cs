using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
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

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "The album genre is required.")]
        public string Genre { get; set; }

        [Required]
        [Range(1, float.MaxValue, ErrorMessage = "Min price is 1$")]
        public float Price { get; set; }

        [Required]
        [Url]
        public string PurchaseUrl { get; set; }

        [Required]
        [Url]
        public string PhotoUrl { get; set; }

        [Required]
        [EnumDataType(typeof(MediaType))]
        public MediaType Media { get; set; }

        [Required(ErrorMessage = "The record label name is required.")]
        public string Label { get; set; }

        [Required(ErrorMessage = "The pressing information is required.")]
        public string Press { get; set; }

        [Required(ErrorMessage = "The album description is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must contain at least 10 characters.")]
        public string Description { get; set; }

        [Required]
        [EnumDataType(typeof(AlbumStatus))]
        public AlbumStatus Status { get; set; }
    }
}
