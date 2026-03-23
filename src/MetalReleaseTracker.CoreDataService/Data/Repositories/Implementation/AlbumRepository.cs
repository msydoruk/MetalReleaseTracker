using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingEntity = await GetAsync(id, cancellationToken);

        if (existingEntity == null)
        {
            return false;
        }

        _dbContext.Albums.Remove(existingEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AlbumEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .FirstOrDefaultAsync(album => album.SKU == sku, cancellationToken);
    }

    public async Task<bool> DeleteBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _dbContext.Albums
            .FirstOrDefaultAsync(album => album.SKU == sku, cancellationToken);

        if (existingEntity == null)
        {
            return false;
        }

        _dbContext.Albums.Remove(existingEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<AlbumEntity>> GetMatchingAlbumsAsync(string canonicalTitle, AlbumMediaType? media, Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Where(album => album.BandId == bandId
                && album.Media == media
                && album.CanonicalTitle != null
                && album.CanonicalTitle.ToLower().Trim() == canonicalTitle.ToLower().Trim())
            .OrderBy(album => album.Price)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AlbumEntity>> GetAlbumsByBandIdAsync(Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Where(album => album.BandId == bandId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AlbumEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .FirstOrDefaultAsync(album => album.Slug == slug, cancellationToken);
    }

    public async Task<List<AlbumSitemapDto>> GetAllAlbumSlugsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Where(album => album.Slug != string.Empty)
            .Select(album => new AlbumSitemapDto
            {
                Slug = album.Slug,
                LastModified = album.LastUpdateDate ?? album.CreatedDate
            })
            .ToListAsync(cancellationToken);
    }
}