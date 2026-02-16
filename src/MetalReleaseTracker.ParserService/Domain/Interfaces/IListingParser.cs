using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IListingParser
{
    DistributorCode DistributorCode { get; }

    Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken);
}
