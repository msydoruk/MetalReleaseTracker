using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class BandReferenceRepository : IBandReferenceRepository
{
    private readonly ParserServiceDbContext _context;

    public BandReferenceRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<BandReferenceEntity?> GetByMetalArchivesIdAsync(long maId, CancellationToken cancellationToken)
    {
        return await _context.BandReferences
            .FirstOrDefaultAsync(b => b.MetalArchivesId == maId, cancellationToken);
    }

    public async Task<List<BandReferenceEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.BandReferences.ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(BandReferenceEntity entity, CancellationToken cancellationToken)
    {
        var existing = await _context.BandReferences
            .FirstOrDefaultAsync(b => b.MetalArchivesId == entity.MetalArchivesId, cancellationToken);

        if (existing != null)
        {
            existing.BandName = entity.BandName;
            existing.Genre = entity.Genre;
            existing.LastSyncedAt = entity.LastSyncedAt;
            _context.BandReferences.Update(existing);
        }
        else
        {
            entity.Id = Guid.NewGuid();
            await _context.BandReferences.AddAsync(entity, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
