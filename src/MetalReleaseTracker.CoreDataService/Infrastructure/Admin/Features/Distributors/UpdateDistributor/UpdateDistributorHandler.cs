using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;

public class UpdateDistributorHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateDistributorHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDistributorDto?> HandleAsync(
        Guid id,
        UpdateDistributorRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Distributors
            .FirstOrDefaultAsync(
                distributor => distributor.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);

        var albumCount = await _context.Albums
            .CountAsync(
                album => album.DistributorId == entity.Id,
                cancellationToken);

        return new AdminDistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code.ToString(),
            AlbumCount = albumCount,
        };
    }
}
