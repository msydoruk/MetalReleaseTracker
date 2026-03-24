using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;

public class GetReviewsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetReviewsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminReviewPagedResult> HandleAsync(
        AdminReviewFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(review =>
                EF.Functions.ILike(review.UserName, $"%{filter.Search}%") ||
                EF.Functions.ILike(review.Message, $"%{filter.Search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(review => review.CreatedDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(review => new AdminReviewDto
            {
                Id = review.Id,
                UserName = review.UserName,
                Message = review.Message,
                CreatedDate = review.CreatedDate,
            })
            .ToListAsync(cancellationToken);

        return new AdminReviewPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
