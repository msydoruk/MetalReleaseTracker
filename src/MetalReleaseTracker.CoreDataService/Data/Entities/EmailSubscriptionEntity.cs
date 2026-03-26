using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("EmailSubscriptions")]
public class EmailSubscriptionEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    [MaxLength(64)]
    public string? VerificationToken { get; set; }

    public DateTime? VerificationTokenExpiresAt { get; set; }

    public DateTime SubscribedAt { get; set; }

    public DateTime? UnsubscribedAt { get; set; }
}
