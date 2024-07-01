using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Subscriptions")]
    public class SubscriptionEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool NotifyForNewReleases { get; set; }
    }
}
