using System.Globalization;
using System.Text.Json;
using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ImportAlbums;

public class ImportAlbumsHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly IBandRepository _bandRepository;
    private readonly IDistributorsRepository _distributorsRepository;
    private readonly ILogger<ImportAlbumsHandler> _logger;

    public ImportAlbumsHandler(
        CoreDataServiceDbContext context,
        IBandRepository bandRepository,
        IDistributorsRepository distributorsRepository,
        ILogger<ImportAlbumsHandler> logger)
    {
        _context = context;
        _bandRepository = bandRepository;
        _distributorsRepository = distributorsRepository;
        _logger = logger;
    }

    public async Task<ImportResult> HandleAsync(
        IFormFile file,
        bool confirm = false,
        CancellationToken cancellationToken = default)
    {
        var result = new ImportResult();
        var rows = await ParseFileAsync(file, cancellationToken);

        foreach (var row in rows)
        {
            result.Processed++;

            if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.BandName))
            {
                result.Errors.Add($"Row {result.Processed}: Missing required fields (Name or BandName)");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.SKU))
            {
                var existing = await _context.Albums
                    .AsNoTracking()
                    .AnyAsync(album => album.SKU == row.SKU, cancellationToken);

                if (existing)
                {
                    result.Skipped++;
                    continue;
                }
            }

            result.Created++;

            if (confirm)
            {
                var bandId = await _bandRepository.GetOrAddAsync(row.BandName, cancellationToken);
                var distributorId = await _distributorsRepository.GetOrAddAsync(
                    row.DistributorName ?? "Unknown",
                    cancellationToken);

                var album = new AlbumEntity
                {
                    Id = Guid.NewGuid(),
                    SKU = row.SKU ?? Guid.NewGuid().ToString(),
                    Name = row.Name,
                    BandId = bandId,
                    DistributorId = distributorId,
                    Genre = row.Genre,
                    Price = row.Price,
                    PurchaseUrl = row.PurchaseUrl ?? string.Empty,
                    PhotoUrl = row.PhotoUrl ?? string.Empty,
                    Label = row.Label ?? string.Empty,
                    Press = row.Press ?? string.Empty,
                    Status = AlbumStatus.New,
                    StockStatus = AlbumStockStatus.InStock,
                    CreatedDate = DateTime.UtcNow,
                    Slug = GenerateSlug(row.BandName, row.Name),
                };

                _context.Albums.Add(album);
            }
        }

        if (confirm)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Imported {Created} albums, skipped {Skipped}", result.Created, result.Skipped);
        }

        return result;
    }

    private static async Task<List<ImportRow>> ParseFileAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync(cancellationToken);

        if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Deserialize<List<ImportRow>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? [];
        }

        var rows = new List<ImportRow>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            return rows;
        }

        for (var i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i].Trim());
            if (fields.Count >= 4)
            {
                rows.Add(new ImportRow
                {
                    SKU = fields.Count > 1 ? fields[1] : null,
                    Name = fields.Count > 2 ? fields[2] : string.Empty,
                    BandName = fields.Count > 3 ? fields[3] : string.Empty,
                    DistributorName = fields.Count > 4 ? fields[4] : null,
                    Genre = fields.Count > 5 ? fields[5] : null,
                    Price = fields.Count > 6 ? float.TryParse(fields[6], CultureInfo.InvariantCulture, out var price) ? price : 0 : 0,
                    PurchaseUrl = fields.Count > 7 ? fields[7] : null,
                    PhotoUrl = fields.Count > 8 ? fields[8] : null,
                    Label = fields.Count > 12 ? fields[12] : null,
                    Press = fields.Count > 13 ? fields[13] : null,
                });
            }
        }

        return rows;
    }

    private static string GenerateSlug(string bandName, string albumName)
    {
        var slug = $"{bandName}-{albumName}"
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", string.Empty)
            .Replace(",", string.Empty);

        return slug.Length > 250 ? slug[..250] : slug;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (var character in line)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (character == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}

public class ImportRow
{
    public string? SKU { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BandName { get; set; } = string.Empty;

    public string? DistributorName { get; set; }

    public string? Genre { get; set; }

    public float Price { get; set; }

    public string? PurchaseUrl { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Label { get; set; }

    public string? Press { get; set; }
}

public class ImportResult
{
    public int Processed { get; set; }

    public int Created { get; set; }

    public int Updated { get; set; }

    public int Skipped { get; set; }

    public List<string> Errors { get; set; } = [];
}
