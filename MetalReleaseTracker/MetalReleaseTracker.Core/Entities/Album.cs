using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Entities
{
    public class Album
    {
        public Guid Id { get; set; }

        public Guid DistributorId { get; set; }

        public Distributor Distributor { get; set; }

        public Guid BandId { get; set; }

        public Band Band { get; set; }

        public string SKU { get; set; }

        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Genre { get; set; }

        public float Price { get; set; }

        public string PurchaseUrl { get; set; }

        public string PhotoUrl { get; set; }

        public MediaType Media { get; set; }

        public string Label { get; set; }

        public string Press { get; set; }

        public string Description { get; set; }

        public AlbumStatus Status { get; set; }

        public bool IsHidden { get; set; }

        public DateTime ModificationTime { get; set; }
    }
}
