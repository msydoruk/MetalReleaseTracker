using System.Globalization;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Parsers
{
    public class AlbumParser
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

        public MediaType ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape,
                _ => MediaType.Unknown
            };
        }

        public DateTime ParseYear(string year)
        {
            if (DateTime.TryParseExact(year?.Trim(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.MinValue;
        }
    }
}
