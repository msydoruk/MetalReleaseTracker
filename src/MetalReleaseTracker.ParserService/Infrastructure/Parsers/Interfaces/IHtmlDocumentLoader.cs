using HtmlAgilityPack;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

public interface IHtmlDocumentLoader
{
    Task<HtmlDocument> LoadHtmlDocumentAsync(string url, CancellationToken cancellationToken);
}