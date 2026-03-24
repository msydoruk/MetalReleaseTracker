using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class UserAlbumWatchRepository : IUserAlbumWatchRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public UserAlbumWatchRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserAlbumWatchEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserAlbumWatches.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string userId, Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UserAlbumWatches
            .FirstOrDefaultAsync(
                watch => watch.UserId == userId && watch.BandId == bandId && watch.CanonicalTitle == canonicalTitle,
                cancellationToken);

        if (entity != null)
        {
            _dbContext.UserAlbumWatches.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string userId, Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAlbumWatches
            .AsNoTracking()
            .AnyAsync(
                watch => watch.UserId == userId && watch.BandId == bandId && watch.CanonicalTitle == canonicalTitle,
                cancellationToken);
    }

    public async Task<Dictionary<string, bool>> GetWatchedKeysAsync(string userId, CancellationToken cancellationToken = default)
    {
        var watches = await _dbContext.UserAlbumWatches
            .AsNoTracking()
            .Where(watch => watch.UserId == userId)
            .Select(watch => new { watch.BandId, watch.CanonicalTitle })
            .ToListAsync(cancellationToken);

        return watches.ToDictionary(
            watch => $"{watch.BandId}:{watch.CanonicalTitle}",
            _ => true);
    }

    public async Task<List<string>> GetWatcherUserIdsAsync(Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAlbumWatches
            .AsNoTracking()
            .Where(watch => watch.BandId == bandId && watch.CanonicalTitle == canonicalTitle)
            .Select(watch => watch.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
