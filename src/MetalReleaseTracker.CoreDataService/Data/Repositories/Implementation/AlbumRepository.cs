using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class AlbumRepository : IAlbumRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public AlbumRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AlbumEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Where(album => album.Id == id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<PagedResultDto<AlbumEntity>> GetFilteredAlbumsAsync(AlbumFilterDto filterCriteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .ApplyAlbumFilters(filterCriteria)
            .ApplyAlbumSorting(filterCriteria.SortBy, filterCriteria.SortAscending);

        return await query.ToPagedResultAsync(filterCriteria.Page, filterCriteria.PageSize, cancellationToken);
    }

    public async Task<List<AlbumEntity>> GetAllFilteredAlbumsAsync(AlbumFilterDto filterCriteria, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .ApplyAlbumFilters(filterCriteria)
            .ApplyAlbumSorting(filterCriteria.SortBy, filterCriteria.SortAscending)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AlbumEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Albums.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AlbumEntity entity, CancellationToken cancellationToken = default)
    {
        var existingEntity = await GetAsync(entity.Id);
        if (existingEntity != null)
        {
            _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        }
        else
        {
            _dbContext.Albums.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingEntity = await GetAsync(id, cancellationToken);

        if (existingEntity != null)
        {
            _dbContext.Albums.Remove(existingEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}