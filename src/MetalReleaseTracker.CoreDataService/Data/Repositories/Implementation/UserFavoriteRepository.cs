using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class UserFavoriteRepository : IUserFavoriteRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public UserFavoriteRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserFavoriteEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserFavorites.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UserFavorites
            .FirstOrDefaultAsync(favorite => favorite.UserId == userId && favorite.AlbumId == albumId, cancellationToken);

        if (entity != null)
        {
            _dbContext.UserFavorites.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFavorites
            .AsNoTracking()
            .AnyAsync(favorite => favorite.UserId == userId && favorite.AlbumId == albumId, cancellationToken);
    }

    public async Task<Dictionary<Guid, UserCollectionStatus>> GetFavoriteAlbumIdsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFavorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId)
            .ToDictionaryAsync(favorite => favorite.AlbumId, favorite => favorite.Status, cancellationToken);
    }

    public async Task<PagedResultDto<AlbumEntity>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, UserCollectionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var favoritesQuery = _dbContext.UserFavorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId);

        if (status.HasValue)
        {
            favoritesQuery = favoritesQuery.Where(favorite => favorite.Status == status.Value);
        }

        var albumIds = favoritesQuery
            .OrderByDescending(favorite => favorite.CreatedDate)
            .Select(favorite => favorite.AlbumId);

        var query = _dbContext.Albums
            .AsNoTracking()
            .Where(album => albumIds.Contains(album.Id))
            .Include(album => album.Band)
            .Include(album => album.Distributor);

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task UpdateStatusAsync(string userId, Guid albumId, UserCollectionStatus status, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UserFavorites
            .FirstOrDefaultAsync(favorite => favorite.UserId == userId && favorite.AlbumId == albumId, cancellationToken);

        if (entity != null)
        {
            entity.Status = status;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
