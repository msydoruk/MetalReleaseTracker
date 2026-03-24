using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("TelegramLinkTokens")]
public class TelegramLinkTokenEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
