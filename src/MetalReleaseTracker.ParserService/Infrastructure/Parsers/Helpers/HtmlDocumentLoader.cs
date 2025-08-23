using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers
{
    public class HtmlDocumentLoader : IHtmlDocumentLoader
    {
        private readonly IHttpRequestService _httpRequestService;

        public HtmlDocumentLoader(IHttpRequestService httpRequestService)
        {
            _httpRequestService = httpRequestService;
        }

        public async Task<HtmlDocument> LoadHtmlDocumentAsync(string url, CancellationToken cancellationToken)
        {
            var pageContent = await _httpRequestService.GetStringWithUserAgentAsync(url, cancellationToken);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContent);

            return htmlDocument;
        }
    }
}