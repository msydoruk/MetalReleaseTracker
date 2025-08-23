using MetalReleaseTracker.ParserService.Domain.Models.Events;

namespace MetalReleaseTracker.ParserService.Domain.Models.Results;

public class PageParsedResult
{
    public IEnumerable<AlbumParsedEvent> ParsedAlbums { get; set; }

    public string NextPageUrl { get; set; }

    public bool HasMorePages => !string.IsNullOrEmpty(NextPageUrl);
}