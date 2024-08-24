using HtmlAgilityPack;

namespace MetalReleaseTracker.Core.Parsers
{
    public static class PaginationHelper
    {
        public static (string nextPageUrl, bool hasMorePages) GetNextPageUrl(HtmlDocument htmlDocument)
        {
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='GtoursPagination']//a[contains(@href, 'page=') and not(contains(@href, 'javascript'))]");
            if (nextPageNode != null)
            {
                string nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(nextPageUrl))
                {
                    return (nextPageUrl, true);
                }
            }

            return (null, false);
        }
    }
}
