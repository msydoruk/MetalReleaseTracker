using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Filters
{
    public class AlbumFilter
    {
        public string BandName {  get; set; }

        public DateTime? ReleaseDateStart { get; set; }

        public DateTime? ReleaseDateEnd { get; set; }

        public string Genre { get; set; }

        public float? PriceMin { get; set; }

        public float? PriceMax { get; set; }

        public AlbumStatus? Status { get; set; }
    }
}
