namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AuditLog.GetAuditLogs;

public class AuditLogDto
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string HttpMethod { get; set; }

    public string RequestPath { get; set; }

    public string Action { get; set; }

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public int ResponseStatusCode { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; }
}
