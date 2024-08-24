using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Parsers
{
    public class AlbumStatusParser
    {
        public AlbumStatus ParseAlbumStatus(string status)
        {
            return status switch
            {
                "New" => AlbumStatus.New,
                "Restock" => AlbumStatus.Restock,
                "Preorder" => AlbumStatus.Preorder,
                _ => AlbumStatus.Unknown
            };
        }
    }
}
