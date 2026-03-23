using System.Globalization;
using System.Text.Json;
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
        var path = uri.AbsolutePath.Trim('/');

        var extensionIndex = path.LastIndexOf(".html", StringComparison.OrdinalIgnoreCase);
        if (extensionIndex >= 0)
        {
            path = path[..extensionIndex];
        }

        var separatorIndex = path.IndexOf("::", StringComparison.Ordinal);
        if (separatorIndex >= 0)
        {
            path = path[..separatorIndex];
        }

        return path.Replace('/', '-');
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

    public static StockStatus ParseStockStatus(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return StockStatus.Unknown;
        }

        var lower = text.ToLowerInvariant();

        if (lower.Contains("out of stock") || lower.Contains("sold out") ||
            lower.Contains("unavailable") || lower.Contains("not available") ||
            lower.Contains("niedostępny") || lower.Contains("wyprzedane"))
        {
            return StockStatus.OutOfStock;
        }

        if (lower.Contains("pre-order") || lower.Contains("preorder") ||
            lower.Contains("pre order") || lower.Contains("przedsprzedaż"))
        {
            return StockStatus.PreOrder;
        }

        if (lower.Contains("in stock") || lower.Contains("available") ||
            lower.Contains("dostępny") || lower.Contains("w magazynie"))
        {
            return StockStatus.InStock;
        }

        return StockStatus.Unknown;
    }

    public static StockStatus ParseStockStatusFromJsonLd(JsonElement? offers)
    {
        if (!offers.HasValue)
        {
            return StockStatus.Unknown;
        }

        var offersElement = offers.Value;
        var availability = ExtractAvailabilityFromOffers(offersElement);

        if (string.IsNullOrWhiteSpace(availability))
        {
            return StockStatus.Unknown;
        }

        if (availability.Contains("OutOfStock", StringComparison.OrdinalIgnoreCase))
        {
            return StockStatus.OutOfStock;
        }

        if (availability.Contains("PreOrder", StringComparison.OrdinalIgnoreCase))
        {
            return StockStatus.PreOrder;
        }

        if (availability.Contains("InStock", StringComparison.OrdinalIgnoreCase))
        {
            return StockStatus.InStock;
        }

        return StockStatus.Unknown;
    }

    private static string? ExtractAvailabilityFromOffers(JsonElement offersElement)
    {
        if (offersElement.ValueKind == JsonValueKind.Object &&
            offersElement.TryGetProperty("availability", out var availElement))
        {
            return availElement.GetString();
        }

        if (offersElement.ValueKind == JsonValueKind.Array && offersElement.GetArrayLength() > 0)
        {
            var firstOffer = offersElement[0];
            if (firstOffer.TryGetProperty("availability", out var arrAvailElement))
            {
                return arrAvailElement.GetString();
            }
        }

        return null;
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