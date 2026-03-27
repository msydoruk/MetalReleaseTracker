using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Utilities;
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

        var slug = SlugGenerator.GenerateSlug(distributorName);
        var slugExists = await _dbContext.Distributors.AnyAsync(distributor => distributor.Slug == slug, cancellationToken);
        if (slugExists)
        {
            var counter = 2;
            while (await _dbContext.Distributors.AnyAsync(distributor => distributor.Slug == $"{slug}-{counter}", cancellationToken))
            {
                counter++;
            }

            slug = $"{slug}-{counter}";
        }

        var newDistributorEntity = new DistributorEntity { Name = distributorName, Slug = slug };
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

    public async Task<DistributorEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Distributors
            .AsNoTracking()
            .Include(distributor => distributor.Translations)
            .FirstOrDefaultAsync(distributor => distributor.Slug == slug, cancellationToken);
    }

    public async Task<List<DistributorWithAlbumCountDto>> GetDistributorsWithAlbumCountAsync(string language, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Distributors
            .AsNoTracking()
            .Select(distributor => new DistributorWithAlbumCountDto
            {
                Id = distributor.Id,
                Name = distributor.Name,
                Slug = distributor.Slug,
                AlbumCount = _dbContext.Albums.Count(album => album.DistributorId == distributor.Id),
                Description = distributor.Translations
                    .Where(t => t.LanguageCode == language)
                    .Select(t => t.Description)
                    .FirstOrDefault()
                    ?? distributor.Translations
                        .Where(t => t.LanguageCode == "en")
                        .Select(t => t.Description)
                        .FirstOrDefault(),
                Country = distributor.Country,
                CountryFlag = distributor.CountryFlag,
                LogoUrl = distributor.LogoUrl,
                WebsiteUrl = distributor.WebsiteUrl,
            })
            .OrderBy(distributorDto => distributorDto.Name)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}