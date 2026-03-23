namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface ISitemapService
{
    Task<string> GenerateSitemapAsync(CancellationToken cancellationToken = default);
}
