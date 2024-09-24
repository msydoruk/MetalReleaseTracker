using System.Globalization;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public static class AlbumParser
    {
        public static AlbumStatus? ParseAlbumStatus(string status)
        {
            return status switch
            {
                "New" => AlbumStatus.New,
                "Restock" => AlbumStatus.Restock,
                "Preorder" => AlbumStatus.Preorder,
                _ => null
            };
        }

        public static MediaType? ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape,
                _ => null
            };
        }

        public static DateTime ParseYear(string year)
        {
            if (DateTime.TryParseExact(year?.Trim(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.MinValue;
        }

        public static float ParsePrice(string priceText)
        {
            if (!string.IsNullOrEmpty(priceText))
            {
                if (float.TryParse(priceText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedPrice))
                {
                    return parsedPrice;
                }
            }

            return 0.0f;
        }
    }
}
