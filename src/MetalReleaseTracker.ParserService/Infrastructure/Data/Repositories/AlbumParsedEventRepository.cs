using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class AlbumParsedEventRepository : IAlbumParsedEventRepository
{
    private readonly ParserServiceDbContext _context;

    public AlbumParsedEventRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlbumParsedEventEntity>> GeAsync(Guid parsingSessionId, CancellationToken cancellationToken)
    {
        return await _context.AlbumParsedEvents
            .Where(album => album.ParsingSessionId == parsingSessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Guid parsingSessionId, string eventPayload, CancellationToken cancellationToken)
    {
        var albumParsedEventEntity = new AlbumParsedEventEntity
        {
            Id = Guid.NewGuid(),
            ParsingSessionId = parsingSessionId,
            CreatedDate = DateTime.UtcNow,
            EventPayload = eventPayload
        };

       await _context.AlbumParsedEvents.AddAsync(albumParsedEventEntity, cancellationToken);
       await _context.SaveChangesAsync(cancellationToken);
    }
}