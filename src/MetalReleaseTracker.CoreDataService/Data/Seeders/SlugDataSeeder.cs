using MetalReleaseTracker.CoreDataService.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Data.Seeders;

public class SlugDataSeeder
{
    private readonly CoreDataServiceDbContext _dbContext;
    private readonly ILogger<SlugDataSeeder> _logger;

    public SlugDataSeeder(CoreDataServiceDbContext dbContext, ILogger<SlugDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedSlugsAsync(CancellationToken cancellationToken = default)
    {
        await SeedBandSlugsAsync(cancellationToken);
        await SeedAlbumSlugsAsync(cancellationToken);
        await CreateUniqueIndexesAsync(cancellationToken);
    }

    private async Task SeedBandSlugsAsync(CancellationToken cancellationToken)
    {
        var bandsWithoutSlug = await _dbContext.Bands
            .Where(band => band.Slug == string.Empty)
            .ToListAsync(cancellationToken);

        if (bandsWithoutSlug.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Seeding slugs for {Count} bands", bandsWithoutSlug.Count);

        var existingSlugs = await _dbContext.Bands
            .Where(band => band.Slug != string.Empty)
            .Select(band => band.Slug)
            .ToListAsync(cancellationToken);

        var slugSet = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);

        foreach (var band in bandsWithoutSlug)
        {
            var slug = ResolveUniqueSlug(SlugGenerator.GenerateSlug(band.Name), slugSet);
            band.Slug = slug;
            slugSet.Add(slug);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Band slugs seeded successfully");
    }

    private async Task SeedAlbumSlugsAsync(CancellationToken cancellationToken)
    {
        var albumsWithoutSlug = await _dbContext.Albums
            .Include(album => album.Band)
            .Where(album => album.Slug == string.Empty)
            .ToListAsync(cancellationToken);

        if (albumsWithoutSlug.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Seeding slugs for {Count} albums", albumsWithoutSlug.Count);

        var existingSlugs = await _dbContext.Albums
            .Where(album => album.Slug != string.Empty)
            .Select(album => album.Slug)
            .ToListAsync(cancellationToken);

        var slugSet = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);

        foreach (var album in albumsWithoutSlug)
        {
            var albumName = album.CanonicalTitle ?? album.Name;
            var slug = ResolveUniqueSlug(
                SlugGenerator.GenerateSlug(album.Band.Name, albumName),
                slugSet);
            album.Slug = slug;
            slugSet.Add(slug);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Album slugs seeded successfully");
    }

    private async Task CreateUniqueIndexesAsync(CancellationToken cancellationToken)
    {
        var hasAlbumIndex = await CheckIndexExistsAsync("IX_Albums_Slug", cancellationToken);
        if (!hasAlbumIndex)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Albums_Slug\" ON \"Albums\" (\"Slug\")",
                cancellationToken);
            _logger.LogInformation("Created unique index IX_Albums_Slug");
        }

        var hasBandIndex = await CheckIndexExistsAsync("IX_Bands_Slug", cancellationToken);
        if (!hasBandIndex)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Bands_Slug\" ON \"Bands\" (\"Slug\")",
                cancellationToken);
            _logger.LogInformation("Created unique index IX_Bands_Slug");
        }
    }

    private async Task<bool> CheckIndexExistsAsync(string indexName, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT 1 FROM pg_indexes WHERE indexname = '{indexName}'";
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result != null;
    }

    private static string ResolveUniqueSlug(string baseSlug, HashSet<string> existingSlugs)
    {
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "unnamed";
        }

        if (!existingSlugs.Contains(baseSlug))
        {
            return baseSlug;
        }

        var suffix = 2;
        while (existingSlugs.Contains($"{baseSlug}-{suffix}"))
        {
            suffix++;
        }

        return $"{baseSlug}-{suffix}";
    }
}
