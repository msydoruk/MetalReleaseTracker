using System.Linq.Expressions;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Extensions;

public static class AlbumFilteringExtensions
{
    public static IQueryable<AlbumEntity> ApplyAlbumFilters(
        this IQueryable<AlbumEntity> query,
        AlbumFilterDto filter)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filter.Name),
                album => album.Name.Contains(filter.Name))
            .WhereIf(filter.MinPrice.HasValue,
                album => album.Price >= (float)filter.MinPrice.Value)
            .WhereIf(filter.MaxPrice.HasValue,
                album => album.Price <= (float)filter.MaxPrice.Value)
            .WhereIf(filter.ReleaseDateFrom.HasValue,
                album => album.ReleaseDate >= filter.ReleaseDateFrom.Value)
            .WhereIf(filter.ReleaseDateTo.HasValue,
                album => album.ReleaseDate <= filter.ReleaseDateTo.Value)
            .WhereIf(filter.BandId.HasValue,
                album => album.BandId == filter.BandId.Value)
            .WhereIf(filter.DistributorId.HasValue,
                album => album.DistributorId == filter.DistributorId.Value)
            .WhereIf(filter.MediaType.HasValue,
                album => album.Media == filter.MediaType.Value)
            .WhereIf(filter.Status.HasValue,
                album => album.Status == filter.Status.Value);
    }

    private static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}