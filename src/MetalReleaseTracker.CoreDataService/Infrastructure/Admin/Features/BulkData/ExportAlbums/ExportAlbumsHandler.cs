using System.Text;
using System.Text.Json;
using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ExportAlbums;

public class ExportAlbumsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public ExportAlbumsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> HandleAsync(
        string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var albums = await _context.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .OrderBy(album => album.Band.Name)
            .ThenBy(album => album.Name)
            .ToListAsync(cancellationToken);

        if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            var jsonData = albums.Select(album => new
            {
                album.Id,
                album.SKU,
                album.Name,
                BandName = album.Band.Name,
                DistributorName = album.Distributor.Name,
                album.Genre,
                album.Price,
                album.PurchaseUrl,
                album.PhotoUrl,
                Media = album.Media.ToString(),
                Status = album.Status.ToString(),
                StockStatus = album.StockStatus.ToString(),
                album.Label,
                album.Press,
                album.CanonicalTitle,
                album.OriginalYear,
                album.Slug,
                album.CreatedDate,
            });

            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            return (Encoding.UTF8.GetBytes(json), "application/json", "albums-export.json");
        }

        var csv = new StringBuilder();
        csv.AppendLine("Id,SKU,Name,BandName,DistributorName,Genre,Price,PurchaseUrl,PhotoUrl,Media,Status,StockStatus,Label,Press,CanonicalTitle,OriginalYear,Slug,CreatedDate");

        foreach (var album in albums)
        {
            csv.Append(album.Id).Append(',');
            csv.Append(CsvEscape(album.SKU)).Append(',');
            csv.Append(CsvEscape(album.Name)).Append(',');
            csv.Append(CsvEscape(album.Band.Name)).Append(',');
            csv.Append(CsvEscape(album.Distributor.Name)).Append(',');
            csv.Append(CsvEscape(album.Genre)).Append(',');
            csv.Append(album.Price).Append(',');
            csv.Append(CsvEscape(album.PurchaseUrl)).Append(',');
            csv.Append(CsvEscape(album.PhotoUrl)).Append(',');
            csv.Append(album.Media).Append(',');
            csv.Append(album.Status).Append(',');
            csv.Append(album.StockStatus).Append(',');
            csv.Append(CsvEscape(album.Label)).Append(',');
            csv.Append(CsvEscape(album.Press)).Append(',');
            csv.Append(CsvEscape(album.CanonicalTitle)).Append(',');
            csv.Append(album.OriginalYear).Append(',');
            csv.Append(CsvEscape(album.Slug)).Append(',');
            csv.AppendLine(album.CreatedDate.ToString("o"));
        }

        return (Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "albums-export.csv");
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
