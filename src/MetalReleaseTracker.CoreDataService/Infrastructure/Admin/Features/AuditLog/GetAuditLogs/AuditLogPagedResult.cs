namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AuditLog.GetAuditLogs;

public class AuditLogPagedResult
{
    public List<AuditLogDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
