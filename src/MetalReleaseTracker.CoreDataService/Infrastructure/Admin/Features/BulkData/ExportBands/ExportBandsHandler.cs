using System.Text;
using System.Text.Json;
using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ExportBands;

public class ExportBandsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public ExportBandsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> HandleAsync(
        string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var bands = await _context.Bands
            .AsNoTracking()
            .OrderBy(band => band.Name)
            .Select(band => new
            {
                band.Id,
                band.Name,
                band.Genre,
                band.PhotoUrl,
                band.MetalArchivesUrl,
                band.FormationYear,
                band.Slug,
                band.IsVisible,
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
            })
            .ToListAsync(cancellationToken);

        if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            var json = JsonSerializer.Serialize(bands, new JsonSerializerOptions { WriteIndented = true });
            return (Encoding.UTF8.GetBytes(json), "application/json", "bands-export.json");
        }

        var csv = new StringBuilder();
        csv.AppendLine("Id,Name,Genre,PhotoUrl,MetalArchivesUrl,FormationYear,Slug,IsVisible,AlbumCount");

        foreach (var band in bands)
        {
            csv.Append(band.Id).Append(',');
            csv.Append(CsvEscape(band.Name)).Append(',');
            csv.Append(CsvEscape(band.Genre)).Append(',');
            csv.Append(CsvEscape(band.PhotoUrl)).Append(',');
            csv.Append(CsvEscape(band.MetalArchivesUrl)).Append(',');
            csv.Append(band.FormationYear).Append(',');
            csv.Append(CsvEscape(band.Slug)).Append(',');
            csv.Append(band.IsVisible).Append(',');
            csv.AppendLine(band.AlbumCount.ToString());
        }

        return (Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "bands-export.csv");
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
