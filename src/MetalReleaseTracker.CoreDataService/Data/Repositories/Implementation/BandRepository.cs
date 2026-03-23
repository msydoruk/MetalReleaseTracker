using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;
using MetalReleaseTracker.CoreDataService.Services.Utilities;
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

        var slug = SlugGenerator.GenerateSlug(bandName);
        var slugExists = await _dbContext.Bands.AnyAsync(
            band => band.Slug == slug, cancellationToken: cancellationToken);
        if (slugExists)
        {
            var suffix = 2;
            while (await _dbContext.Bands.AnyAsync(
                band => band.Slug == $"{slug}-{suffix}", cancellationToken: cancellationToken))
            {
                suffix++;
            }

            slug = $"{slug}-{suffix}";
        }

        var newBandEntity = new BandEntity { Name = bandName, Slug = slug };
        await _dbContext.Bands.AddAsync(newBandEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
                Description = band.Description,
                PhotoUrl = band.PhotoUrl,
                Genre = band.Genre,
                MetalArchivesUrl = band.MetalArchivesUrl,
                Slug = band.Slug,
                AlbumCount = _dbContext.Albums.Count(album => album.BandId == band.Id)
            })
            .OrderBy(bandDto => bandDto.Name)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<BandEntity?> GetByNameAsync(string bandName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .FirstOrDefaultAsync(band => band.Name.ToUpper() == bandName.ToUpper(), cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(BandEntity band, CancellationToken cancellationToken = default)
    {
        _dbContext.Bands.Update(band);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<string>> GetDistinctGenresAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .Where(band => band.Genre != null && band.Genre != string.Empty)
            .Select(band => band.Genre!)
            .Distinct()
            .OrderBy(genre => genre)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BandEntity>> GetBandsByGenreAsync(string genre, Guid excludeBandId, int limit, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .Where(band => band.Id != excludeBandId
                && band.Genre != null
                && EF.Functions.ILike(band.Genre, $"%{genre}%"))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<BandEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .FirstOrDefaultAsync(band => band.Slug == slug, cancellationToken);
    }

    public async Task<List<BandSitemapDto>> GetAllBandSlugsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bands
            .AsNoTracking()
            .Where(band => band.Slug != string.Empty)
            .Select(band => new BandSitemapDto { Slug = band.Slug })
            .ToListAsync(cancellationToken);
    }
}