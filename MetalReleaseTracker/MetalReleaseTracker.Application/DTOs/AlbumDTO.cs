using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.DTOs
{
    public class AlbumDto
    {
        public string BandName { get; set; }

        public string SKU { get; set; }

        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Genre { get; set; }

        public float Price { get; set; }

        public string PurchaseUrl { get; set; }

        public string PhotoUrl { get; set; }

        public MediaType? Media { get; set; }

        public string Label { get; set; }

        public string Press { get; set; }

        public string Description { get; set; }

        public AlbumStatus? Status { get; set; }
    }
}
