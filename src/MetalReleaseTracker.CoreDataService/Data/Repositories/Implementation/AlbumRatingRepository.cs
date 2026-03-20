using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class AlbumRatingRepository : IAlbumRatingRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public AlbumRatingRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AlbumRatingEntity?> GetAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AlbumRatings
            .AsNoTracking()
            .FirstOrDefaultAsync(rating => rating.UserId == userId && rating.AlbumId == albumId, cancellationToken);
    }

    public async Task AddOrUpdateAsync(AlbumRatingEntity entity, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.AlbumRatings
            .FirstOrDefaultAsync(rating => rating.UserId == entity.UserId && rating.AlbumId == entity.AlbumId, cancellationToken);

        if (existing != null)
        {
            existing.Rating = entity.Rating;
            existing.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            await _dbContext.AlbumRatings.AddAsync(entity, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.AlbumRatings
            .FirstOrDefaultAsync(rating => rating.UserId == userId && rating.AlbumId == albumId, cancellationToken);

        if (entity != null)
        {
            _dbContext.AlbumRatings.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<double?> GetAverageRatingAsync(Guid albumId, CancellationToken cancellationToken = default)
    {
        var ratings = _dbContext.AlbumRatings
            .AsNoTracking()
            .Where(rating => rating.AlbumId == albumId);

        if (!await ratings.AnyAsync(cancellationToken))
        {
            return null;
        }

        return await ratings.AverageAsync(rating => rating.Rating, cancellationToken);
    }

    public async Task<int> GetRatingCountAsync(Guid albumId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AlbumRatings
            .AsNoTracking()
            .CountAsync(rating => rating.AlbumId == albumId, cancellationToken);
    }
}
