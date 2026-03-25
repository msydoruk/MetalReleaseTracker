namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;

public interface IAiSeoService
{
    Task<AiSeoResult> GenerateBandSeoAsync(Guid bandId, CancellationToken cancellationToken = default);

    Task<AiSeoResult> GenerateAlbumSeoAsync(Guid albumId, CancellationToken cancellationToken = default);

    Task<int> GenerateBulkBandSeoAsync(int limit, CancellationToken cancellationToken = default);

    Task<int> GenerateBulkAlbumSeoAsync(int limit, CancellationToken cancellationToken = default);
}

public class AiSeoResult
{
    public bool Success { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public string? Error { get; set; }
}
