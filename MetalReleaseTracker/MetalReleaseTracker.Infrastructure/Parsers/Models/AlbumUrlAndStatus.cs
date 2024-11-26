using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Infrastructure.Parsers.Models
{
    public class AlbumUrlAndStatus
    {
        public string Url { get; set; }

        public AlbumStatus? Status { get; set; }
    }
}
