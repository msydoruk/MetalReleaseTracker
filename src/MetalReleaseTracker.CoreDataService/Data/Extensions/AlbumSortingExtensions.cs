using System.Linq.Expressions;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Data.Extensions;

public static class AlbumSortingExtensions
{
    private static readonly
        Dictionary<AlbumSortField, (Expression<Func<AlbumEntity, object>> Ascending,
            Expression<Func<AlbumEntity, object>> Descending)> SortExpressions = new()
        {
            [AlbumSortField.Name] = (album => album.Name, album => album.Name),
            [AlbumSortField.Price] = (album => album.Price, album => album.Price),
            [AlbumSortField.ReleaseDate] = (album => album.ReleaseDate, album => album.ReleaseDate),
            [AlbumSortField.Band] = (album => album.Band.Name, album => album.Band.Name),
            [AlbumSortField.Distributor] = (album => album.Distributor.Name, album => album.Distributor.Name),
            [AlbumSortField.Media] = (album => album.Media, album => album.Media),
            [AlbumSortField.Status] = (album => album.Status, album => album.Status),
            [AlbumSortField.OriginalYear] = (album => album.OriginalYear, album => album.OriginalYear)
        };

    public static IQueryable<AlbumEntity> ApplyAlbumSorting(
        this IQueryable<AlbumEntity> query,
        AlbumSortField? sortBy,
        bool sortAscending)
    {
        var field = sortBy ?? AlbumSortField.OriginalYear;

        if (!SortExpressions.TryGetValue(field, out var sortExpressions))
        {
            sortExpressions = SortExpressions[AlbumSortField.OriginalYear];
        }

        return sortAscending
            ? query.OrderBy(sortExpressions.Ascending)
            : query.OrderByDescending(sortExpressions.Descending);
    }
}