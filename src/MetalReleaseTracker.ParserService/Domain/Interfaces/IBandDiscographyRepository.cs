using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IBandDiscographyRepository
{
    Task<Dictionary<string, HashSet<string>>> GetAllGroupedByBandNameAsync(CancellationToken cancellationToken);

    Task ReplaceForBandAsync(Guid bandReferenceId, List<BandDiscographyEntity> entries, CancellationToken cancellationToken);
}
