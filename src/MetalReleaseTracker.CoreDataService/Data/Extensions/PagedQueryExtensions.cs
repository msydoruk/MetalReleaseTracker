using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Extensions;

public static class PagedQueryExtensions
{
    public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken) where T : class
    {
        var totalCount = await query.CountAsync(cancellationToken: cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageCount = pageCount,
            PageSize = pageSize,
            CurrentPage = page
        };
    }
}