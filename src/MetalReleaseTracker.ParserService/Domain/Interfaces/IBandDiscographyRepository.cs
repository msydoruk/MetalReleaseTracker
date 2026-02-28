using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IBandDiscographyRepository
{
    Task<Dictionary<string, HashSet<string>>> GetAllGroupedByBandNameAsync(CancellationToken cancellationToken);

    Task<HashSet<string>> GetAllBandNamesAsync(CancellationToken cancellationToken);

    Task<Dictionary<Guid, int>> GetAlbumCountsByBandReferenceAsync(CancellationToken cancellationToken);

    Task ReplaceForBandAsync(Guid bandReferenceId, List<BandDiscographyEntity> entries, CancellationToken cancellationToken);
}
