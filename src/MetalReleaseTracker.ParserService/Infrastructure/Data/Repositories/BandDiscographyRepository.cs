using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;

public class BandDiscographyRepository : IBandDiscographyRepository
{
    private readonly ParserServiceDbContext _context;

    public BandDiscographyRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetAllBandNamesAsync(CancellationToken cancellationToken)
    {
        var bandNames = await _context.BandReferences
            .Select(b => b.BandName)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(bandNames, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Dictionary<string, HashSet<string>>> GetAllGroupedByBandNameAsync(CancellationToken cancellationToken)
    {
        var bandNames = await _context.BandReferences
            .Select(b => b.BandName)
            .ToListAsync(cancellationToken);

        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var bandName in bandNames)
        {
            if (!result.ContainsKey(bandName))
            {
                result[bandName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        var discographyEntries = await _context.BandDiscography
            .Include(d => d.BandReference)
            .Select(d => new { d.BandReference.BandName, d.NormalizedAlbumTitle })
            .ToListAsync(cancellationToken);

        foreach (var entry in discographyEntries)
        {
            if (result.TryGetValue(entry.BandName, out var albumTitles))
            {
                albumTitles.Add(entry.NormalizedAlbumTitle);
            }
            else
            {
                result[entry.BandName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { entry.NormalizedAlbumTitle };
            }
        }

        return result;
    }

    public async Task ReplaceForBandAsync(Guid bandReferenceId, List<BandDiscographyEntity> entries, CancellationToken cancellationToken)
    {
        var existing = await _context.BandDiscography
            .Where(d => d.BandReferenceId == bandReferenceId)
            .ToListAsync(cancellationToken);

        _context.BandDiscography.RemoveRange(existing);

        if (entries.Count > 0)
        {
            await _context.BandDiscography.AddRangeAsync(entries, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
