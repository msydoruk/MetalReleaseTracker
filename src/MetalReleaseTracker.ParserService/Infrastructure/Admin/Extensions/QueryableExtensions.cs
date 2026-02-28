using System.Linq.Expressions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
        where T : class
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageCount = pageCount,
            PageSize = pageSize,
            CurrentPage = page,
        };
    }
}
