using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("AuditLogs")]
public class AuditLogEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string? UserId { get; set; }

    [MaxLength(200)]
    public string? UserName { get; set; }

    [Required]
    [MaxLength(10)]
    public string HttpMethod { get; set; }

    [Required]
    [MaxLength(500)]
    public string RequestPath { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(200)]
    public string? EntityId { get; set; }

    public int ResponseStatusCode { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }
}
