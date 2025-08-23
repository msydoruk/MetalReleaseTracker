using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IParser
{
    DistributorCode DistributorCode { get; }

    Task<PageParsedResult> ParseAsync(string parsingUrl, CancellationToken cancellationToken);
}