using System.Globalization;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class AlbumParser
    {
        public AlbumStatus? ParseAlbumStatus(string status)
        {
            return status switch
            {
                "New" => AlbumStatus.New,
                "Restock" => AlbumStatus.Restock,
                "Preorder" => AlbumStatus.Preorder,
                _ => null
            };
        }

        public MediaType? ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape,
                _ => null
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
