using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface ICatalogueIndexDetailRepository
{
    Task<CatalogueIndexDetailEntity?> GetByCatalogueIndexIdAsync(Guid catalogueIndexId, CancellationToken cancellationToken);

    Task<List<CatalogueIndexDetailEntity>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken);

    Task AddAsync(CatalogueIndexDetailEntity entity, CancellationToken cancellationToken);

    Task UpdateAsync(CatalogueIndexDetailEntity entity, CancellationToken cancellationToken);
}
