using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Filters
{
    public class AlbumFilterResult
    {
        public IEnumerable<Album> Albums { get; set; }

        public int TotalCount { get; set; }
    }
}
