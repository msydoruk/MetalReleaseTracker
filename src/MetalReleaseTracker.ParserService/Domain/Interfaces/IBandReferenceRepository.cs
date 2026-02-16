using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IBandReferenceRepository
{
    Task<BandReferenceEntity?> GetByMetalArchivesIdAsync(long maId, CancellationToken cancellationToken);

    Task<List<BandReferenceEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task UpsertAsync(BandReferenceEntity entity, CancellationToken cancellationToken);
}
