using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;

public interface IAlbumParsedEventRepository
{
    Task<List<AlbumParsedEventEntity>> GeAsync(Guid parsingSessionId, CancellationToken cancellationToken);

    Task AddAsync(Guid parsingSessionId, string eventPayload, CancellationToken cancellationToken);
}