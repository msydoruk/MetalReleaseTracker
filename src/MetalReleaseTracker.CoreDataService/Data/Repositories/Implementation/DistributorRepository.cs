using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class DistributorRepository : IDistributorsRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public DistributorRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> GetOrAddAsync(string distributorName, CancellationToken cancellationToken = default)
    {
        var existingDistributorEntity = await _dbContext.Distributors
            .AsNoTracking()
            .FirstOrDefaultAsync(distributor => distributor.Name == distributorName, cancellationToken: cancellationToken);

        if (existingDistributorEntity != null)
        {
            return existingDistributorEntity.Id;
        }

        var newDistributorEntity = new DistributorEntity { Name = distributorName };
        await _dbContext.Distributors.AddAsync(newDistributorEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newDistributorEntity.Id;
    }

    public async Task<List<DistributorEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Distributors.AsNoTracking().OrderBy(distributor => distributor.Name).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<DistributorEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Distributors.FindAsync(id, cancellationToken);
    }

    public async Task<List<DistributorWithAlbumCountDto>> GetDistributorsWithAlbumCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Distributors
            .AsNoTracking()
            .Select(distributor => new DistributorWithAlbumCountDto
            {
                Id = distributor.Id,
                Name = distributor.Name,
                AlbumCount = _dbContext.Albums.Count(album => album.DistributorId == distributor.Id)
            })
            .OrderBy(distributorDto => distributorDto.Name)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}