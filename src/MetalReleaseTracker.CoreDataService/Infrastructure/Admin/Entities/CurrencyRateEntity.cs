using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;

[Table("CurrencyRates")]
public class CurrencyRateEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(3)]
    public string Code { get; set; }

    [Required]
    [MaxLength(5)]
    public string Symbol { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal RateToEur { get; set; }

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
