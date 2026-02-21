using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class CatalogueIndexRepository : ICatalogueIndexRepository
{
    private readonly ParserServiceDbContext _context;

    public CatalogueIndexRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CatalogueIndexEntity?> GetByDetailUrlAsync(
        DistributorCode code,
        string detailUrl,
        CancellationToken cancellationToken)
    {
        return await _context.CatalogueIndex
            .FirstOrDefaultAsync(e => e.DistributorCode == code && e.DetailUrl == detailUrl, cancellationToken);
    }

    public async Task<List<CatalogueIndexEntity>> GetByStatusAsync(
        DistributorCode code,
        CatalogueIndexStatus status,
        CancellationToken cancellationToken)
    {
        return await _context.CatalogueIndex
            .Where(e => e.DistributorCode == code && e.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CatalogueIndexEntity>> GetByStatusAsync(
        CatalogueIndexStatus status,
        CancellationToken cancellationToken)
    {
        return await _context.CatalogueIndex
            .Where(e => e.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(CatalogueIndexEntity entity, CancellationToken cancellationToken)
    {
        var existing = await _context.CatalogueIndex
            .FirstOrDefaultAsync(
                e => e.DistributorCode == entity.DistributorCode && e.DetailUrl == entity.DetailUrl,
                cancellationToken);

        if (existing != null)
        {
            existing.BandName = entity.BandName;
            existing.AlbumTitle = entity.AlbumTitle;
            existing.RawTitle = entity.RawTitle;
            existing.MediaType = entity.MediaType;
            existing.BandReferenceId = entity.BandReferenceId;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.CatalogueIndex.Update(existing);
        }
        else
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.CatalogueIndex.AddAsync(entity, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, CatalogueIndexStatus status, CancellationToken cancellationToken)
    {
        var entity = await _context.CatalogueIndex.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity != null)
        {
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.CatalogueIndex.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateStatusBatchAsync(IEnumerable<Guid> ids, CatalogueIndexStatus status, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        var entities = await _context.CatalogueIndex
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
