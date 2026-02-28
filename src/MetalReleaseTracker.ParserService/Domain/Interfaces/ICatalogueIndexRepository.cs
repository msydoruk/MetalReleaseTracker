using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface ICatalogueIndexRepository
{
    Task<CatalogueIndexEntity?> GetByDetailUrlAsync(DistributorCode code, string detailUrl, CancellationToken cancellationToken);

    Task<List<CatalogueIndexEntity>> GetByStatusAsync(DistributorCode code, CatalogueIndexStatus status, CancellationToken cancellationToken);

    Task<List<CatalogueIndexEntity>> GetByStatusAsync(CatalogueIndexStatus status, CancellationToken cancellationToken);

    Task<List<CatalogueIndexEntity>> GetByStatusesWithDiscographyAsync(DistributorCode code, IEnumerable<CatalogueIndexStatus> statuses, CancellationToken cancellationToken);

    Task UpsertAsync(CatalogueIndexEntity entity, CancellationToken cancellationToken);

    Task UpdateStatusAsync(Guid id, CatalogueIndexStatus status, CancellationToken cancellationToken);

    Task UpdateStatusBatchAsync(IEnumerable<Guid> ids, CatalogueIndexStatus status, CancellationToken cancellationToken);
}
