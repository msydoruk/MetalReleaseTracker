using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class AlbumChangeLogRepository : IAlbumChangeLogRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public AlbumChangeLogRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AlbumChangeLogEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.AlbumChangeLogs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultDto<AlbumChangeLogEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AlbumChangeLogs
            .AsNoTracking()
            .OrderByDescending(changeLog => changeLog.ChangedAt);

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<List<AlbumChangeLogEntity>> GetByAlbumNameAsync(string albumName, string bandName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AlbumChangeLogs
            .AsNoTracking()
            .Where(changeLog => changeLog.AlbumName.ToLower() == albumName.ToLower()
                && changeLog.BandName.ToLower() == bandName.ToLower())
            .OrderBy(changeLog => changeLog.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
