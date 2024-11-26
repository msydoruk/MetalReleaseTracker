using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Filters
{
    public class AlbumFilter
    {
        public string BandName {  get; set; }

        public DateTime? ReleaseDateStart { get; set; }

        public DateTime? ReleaseDateEnd { get; set; }

        public float? MinimumPrice { get; set; }

        public float? MaximumPrice { get; set; }

        public AlbumStatus? Status { get; set; }
    }
}
