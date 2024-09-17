using HtmlAgilityPack;

namespace MetalReleaseTracker.Infrastructure.Utils
{
    public interface IHtmlLoader
    {
        Task<HtmlDocument> LoadHtmlDocumentAsync(string url);
    }
}
