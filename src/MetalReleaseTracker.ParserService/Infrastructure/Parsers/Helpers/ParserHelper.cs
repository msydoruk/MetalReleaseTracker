using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public static class ParserHelper
{
    public static async Task<HtmlDocument> LoadHtmlDocumentOrThrowAsync(
        IHtmlDocumentLoader loader,
        string url,
        ILogger logger,
        Func<string, Exception> exceptionFactory,
        CancellationToken cancellationToken)
    {
        var htmlDocument = await loader.LoadHtmlDocumentAsync(url, cancellationToken);

        if (htmlDocument?.DocumentNode == null)
        {
            var error = $"Failed to load or parse the HTML document {url}.";
            logger.LogError(error);
            throw exceptionFactory(error);
        }

        return htmlDocument;
    }

    public static async Task DelayBetweenRequestsAsync(
        GeneralParserSettings settings,
        Random random,
        CancellationToken cancellationToken)
    {
        var delaySeconds = random.Next(
            settings.MinDelayBetweenRequestsSeconds,
            settings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    public static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);
        var text = document.DocumentNode.InnerText ?? string.Empty;
        text = HtmlEntity.DeEntitize(text);
        text = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", string.Empty);

        return text.Trim();
    }

    public static JsonElement? ExtractProductJsonLd(HtmlDocument htmlDocument)
    {
        var scriptNodes = htmlDocument.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
        if (scriptNodes == null)
        {
            return null;
        }

        foreach (var scriptNode in scriptNodes)
        {
            var json = scriptNode.InnerText?.Trim();
            if (string.IsNullOrEmpty(json))
            {
                continue;
            }

            try
            {
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("@type", out var typeElement) &&
                    typeElement.GetString() == "Product")
                {
                    return root;
                }

                if (root.TryGetProperty("@graph", out var graph) &&
                    graph.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in graph.EnumerateArray())
                    {
                        if (item.TryGetProperty("@type", out var itemType) &&
                            itemType.GetString() == "Product")
                        {
                            return item;
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        return null;
    }
}
