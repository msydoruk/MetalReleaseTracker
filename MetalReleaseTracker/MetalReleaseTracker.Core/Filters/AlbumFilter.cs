using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Filters
{
    public class AlbumFilter
    {
        public Guid? DistributorId { get; set; }

        public Guid? BandId { get; set; }

        public string? AlbumName {  get; set; }

        public DateTime? ReleaseDateStart { get; set; }

        public DateTime? ReleaseDateEnd { get; set; }

        public float? MinimumPrice { get; set; }

        public float? MaximumPrice { get; set; }

        public AlbumStatus? Status { get; set; }

        public MediaType? Media { get; set; }
    }
}
