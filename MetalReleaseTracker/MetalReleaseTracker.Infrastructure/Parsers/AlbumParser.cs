using System.Globalization;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class AlbumParser
    {
        public AlbumStatus ParseAlbumStatus(string status)
        {
            return status switch
            {
                "New" => AlbumStatus.New,
                "Restock" => AlbumStatus.Restock,
                "Preorder" => AlbumStatus.Preorder
            };
        }

        public MediaType ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape
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

        public float ParsePrice(string priceText)
        {
            priceText = priceText?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();

            if (!string.IsNullOrEmpty(priceText))
            {
                if (float.TryParse(priceText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedPrice))
                {
                    return parsedPrice;
                }
                else
                {
                    throw new OsmoseProductionsParserException($"Failed to parse price: {priceText}");
                }
            }

            return 0.0f;
        }
    }
}
