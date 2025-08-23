using System.Globalization;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public static class AlbumParsingHelper
{
    public static AlbumStatus? ParseAlbumStatus(string status)
    {
        return status.ToUpper() switch
        {
            "NEW" => AlbumStatus.New,
            "RESTOCK" => AlbumStatus.Restock,
            "PREORDER" => AlbumStatus.PreOrder,
            _ => null
        };
    }

    public static AlbumMediaType? ParseMediaType(string mediaType)
    {
        var upperMediaType = mediaType.ToUpper();

        var mediaTypes = new Dictionary<string, AlbumMediaType>()
        {
            { "CD", AlbumMediaType.CD },
            { "LP", AlbumMediaType.LP },
            { "TAPE", AlbumMediaType.Tape }
        };

        foreach (var pair in mediaTypes)
        {
            string keyword = pair.Key;
            AlbumMediaType enumValue = pair.Value;

            if (upperMediaType.Contains(" " + keyword + " ") || upperMediaType.StartsWith(keyword + " ") ||
                upperMediaType.EndsWith(" " + keyword))
            {
                return enumValue;
            }
        }

        return null;
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