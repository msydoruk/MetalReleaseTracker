using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class CatalogueIndexDetailRepository : ICatalogueIndexDetailRepository
{
    private readonly ParserServiceDbContext _context;

    public CatalogueIndexDetailRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CatalogueIndexDetailEntity?> GetByCatalogueIndexIdAsync(
        Guid catalogueIndexId,
        CancellationToken cancellationToken)
    {
        return await _context.CatalogueIndexDetails
            .FirstOrDefaultAsync(e => e.CatalogueIndexId == catalogueIndexId, cancellationToken);
    }

    public async Task<List<CatalogueIndexDetailEntity>> GetUnpublishedAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await _context.CatalogueIndexDetails
            .Include(e => e.CatalogueIndex)
                .ThenInclude(e => e.BandReference)
            .Where(e => e.ChangeType != ChangeType.Active && e.PublicationStatus == PublicationStatus.Unpublished)
            .OrderBy(e => e.UpdatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CatalogueIndexDetailEntity entity, CancellationToken cancellationToken)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.CatalogueIndexDetails.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CatalogueIndexDetailEntity entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.CatalogueIndexDetails.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
