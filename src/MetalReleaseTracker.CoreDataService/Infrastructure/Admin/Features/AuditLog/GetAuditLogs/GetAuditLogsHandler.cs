using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AuditLog.GetAuditLogs;

public class GetAuditLogsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetAuditLogsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLogPagedResult> HandleAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPattern = $"%{filter.Search}%";
            query = query.Where(log =>
                EF.Functions.ILike(log.Action, searchPattern) ||
                (log.UserName != null && EF.Functions.ILike(log.UserName, searchPattern)) ||
                EF.Functions.ILike(log.RequestPath, searchPattern) ||
                (log.Details != null && EF.Functions.ILike(log.Details, searchPattern)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(log => log.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            query = query.Where(log => log.EntityType == filter.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            query = query.Where(log => log.UserId == filter.UserId);
        }

        if (filter.From.HasValue)
        {
            query = query.Where(log => log.Timestamp >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(log => log.Timestamp <= filter.To.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                HttpMethod = log.HttpMethod,
                RequestPath = log.RequestPath,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                ResponseStatusCode = log.ResponseStatusCode,
                Details = log.Details,
                Timestamp = log.Timestamp,
            })
            .ToListAsync(cancellationToken);

        return new AuditLogPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
