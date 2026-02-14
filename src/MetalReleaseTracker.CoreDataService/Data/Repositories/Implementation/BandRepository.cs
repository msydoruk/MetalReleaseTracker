using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class BandRepository : IBandRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public BandRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> GetOrAddAsync(string bandName, CancellationToken cancellationToken = default)
    {
        var existingBandEntity =
            await _dbContext.Bands
                .AsNoTracking()
                .FirstOrDefaultAsync(band => band.Name.ToUpper() == bandName.ToUpper(), cancellationToken: cancellationToken);

        if (existingBandEntity != null)
        {
            return existingBandEntity.Id;
        }

        var newBandEntity = new BandEntity { Name = bandName };
        await _dbContext.Bands.AddAsync(newBandEntity);
        await _dbContext.SaveChangesAsync();

        return newBandEntity.Id;
    }

    public async Task<List<BandEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .OrderBy(band => band.Name).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<BandEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands.FindAsync(id, cancellationToken);
    }

    public async Task<List<BandWithAlbumCountDto>> GetBandsWithAlbumCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .Select(band => new BandWithAlbumCountDto
            {
                Id = band.Id,
                Name = band.Name,
                AlbumCount = _dbContext.Albums.Count(album => album.BandId == band.Id)
            })
            .OrderBy(bandDto => bandDto.Name)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}