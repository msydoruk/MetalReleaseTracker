using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class ParsingSessionRepository : IParsingSessionRepository
{
    private readonly ParserServiceDbContext _context;
    private readonly ILogger<ParsingSessionRepository> _logger;

    public ParsingSessionRepository(ParserServiceDbContext context, ILogger<ParsingSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<ParsingSessionEntity?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _context.ParsingSessions.FirstOrDefaultAsync(state => state.Id == id, cancellationToken);
    }

    public async Task<ParsingSessionEntity?> GetIncompleteAsync(DistributorCode distributorCode, CancellationToken cancellationToken)
    {
        var parsingSessionEntity = await _context.ParsingSessions.FirstOrDefaultAsync(
            state => state.DistributorCode == distributorCode && state.ParsingStatus == AlbumParsingStatus.Incomplete,
            cancellationToken);

        return parsingSessionEntity;
    }

    public async Task<List<ParsingSessionEntity>> GetParsedAsync(CancellationToken cancellationToken)
    {
        return await _context.ParsingSessions
            .Where(state => state.ParsingStatus == AlbumParsingStatus.Parsed)
            .ToListAsync(cancellationToken);
    }

    public async Task<ParsingSessionEntity> AddAsync(DistributorCode distributorCode, CancellationToken cancellationToken)
    {
        var parsingSessionEntity = new ParsingSessionEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = distributorCode,
            LastUpdatedDate = DateTime.UtcNow
        };

        await _context.ParsingSessions.AddAsync(parsingSessionEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return parsingSessionEntity;
    }

    public async Task<bool> UpdateParsingStatus(Guid id, AlbumParsingStatus parsingStatus, CancellationToken cancellationToken)
    {
        var parsingSessionEntity = await _context.ParsingSessions.FirstOrDefaultAsync(state => state.Id == id, cancellationToken);

        if (parsingSessionEntity == null)
        {
            return false;
        }

        parsingSessionEntity.ParsingStatus = parsingStatus;
        parsingSessionEntity.LastUpdatedDate = DateTime.UtcNow;

        _context.ParsingSessions.Update(parsingSessionEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}