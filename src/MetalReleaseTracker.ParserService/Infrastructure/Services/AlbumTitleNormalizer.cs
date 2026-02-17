using System.Text.RegularExpressions;

namespace MetalReleaseTracker.ParserService.Infrastructure.Services;

public static partial class AlbumTitleNormalizer
{
    public static string Normalize(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var lowered = title.ToLowerInvariant();
        var stripped = PunctuationRegex().Replace(lowered, string.Empty);
        var collapsed = WhitespaceRegex().Replace(stripped, " ");

        return collapsed.Trim();
    }

    [GeneratedRegex(@"[\-'\"":()\[\].,!?]")]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
