namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface ISeoMetaTagService
{
    Task<string> GetHtmlWithMetaTags(string path, CancellationToken cancellationToken = default);
}
