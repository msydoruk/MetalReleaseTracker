using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class UserFollowedBandRepository : IUserFollowedBandRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public UserFollowedBandRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserFollowedBandEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserFollowedBands.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(string userId, Guid bandId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UserFollowedBands
            .FirstOrDefaultAsync(follow => follow.UserId == userId && follow.BandId == bandId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        _dbContext.UserFollowedBands.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(string userId, Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFollowedBands
            .AsNoTracking()
            .AnyAsync(follow => follow.UserId == userId && follow.BandId == bandId, cancellationToken);
    }

    public async Task<Dictionary<Guid, bool>> GetFollowedBandIdsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFollowedBands
            .AsNoTracking()
            .Where(follow => follow.UserId == userId)
            .ToDictionaryAsync(follow => follow.BandId, follow => true, cancellationToken);
    }

    public async Task<List<UserFollowedBandEntity>> GetFollowedBandsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFollowedBands
            .AsNoTracking()
            .Include(follow => follow.Band)
            .Where(follow => follow.UserId == userId)
            .OrderByDescending(follow => follow.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFollowerCountAsync(Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserFollowedBands
            .AsNoTracking()
            .CountAsync(follow => follow.BandId == bandId, cancellationToken);
    }

    public async Task<PagedResultDto<AlbumEntity>> GetFeedAlbumsAsync(List<Guid> bandIds, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Albums
            .AsNoTracking()
            .Where(album => bandIds.Contains(album.BandId))
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .OrderByDescending(album => album.CreatedDate);

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
