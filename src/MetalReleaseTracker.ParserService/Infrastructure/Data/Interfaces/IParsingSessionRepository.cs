using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;

public interface IParsingSessionRepository
{
    Task<ParsingSessionEntity?> GetById(Guid id, CancellationToken cancellationToken);

    Task<ParsingSessionEntity?> GetIncompleteAsync(DistributorCode distributorCode, CancellationToken cancellationToken);

    Task<List<ParsingSessionEntity>> GetParsedAsync(CancellationToken cancellationToken);

    Task<ParsingSessionEntity> AddAsync(DistributorCode distributorCode, CancellationToken cancellationToken);

    Task<bool> UpdateParsingStatus(Guid parsingSessionId, AlbumParsingStatus parsingStatus, CancellationToken cancellationToken);
}