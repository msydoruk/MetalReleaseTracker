using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Results;

public class ListingPageResult
{
    public List<ListingItem> Listings { get; set; } = [];

    public string NextPageUrl { get; set; }

    public bool HasMorePages => !string.IsNullOrEmpty(NextPageUrl);
}
