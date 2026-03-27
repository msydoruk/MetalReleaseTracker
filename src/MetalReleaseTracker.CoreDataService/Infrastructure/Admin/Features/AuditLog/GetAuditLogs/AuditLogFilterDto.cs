namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AuditLog.GetAuditLogs;

public class AuditLogFilterDto
{
    public string? Search { get; set; }

    public string? Action { get; set; }

    public string? EntityType { get; set; }

    public string? UserId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
