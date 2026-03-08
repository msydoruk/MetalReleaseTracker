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

    public static string GenerateSkuFromUrl(string url)
    {
        var uri = new Uri(url);
        var lastSegment = uri.Segments[^1].TrimEnd('/');
        var slug = Path.GetFileNameWithoutExtension(lastSegment);

        var separatorIndex = slug.IndexOf("::", StringComparison.Ordinal);
        if (separatorIndex >= 0)
        {
            slug = slug[..separatorIndex];
        }

        return slug;
    }

    public static string? TruncateName(string? value) => Truncate(value, 500);

    public static string? TruncateGenre(string? value) => Truncate(value, 500);

    public static string? TruncateLabel(string? value) => Truncate(value, 500);

    public static string? TruncatePress(string? value) => Truncate(value, 500);

    public static string? TruncateSku(string? value) => Truncate(value, 200);

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

    private static string? Truncate(string? input, int maxLength)
    {
        if (input == null || input.Length <= maxLength)
        {
            return input;
        }

        return input[..maxLength];
    }
}