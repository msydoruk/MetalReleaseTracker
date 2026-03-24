namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.UpdateCurrency;

public class UpdateCurrencyRequest
{
    public string? Code { get; set; }

    public string? Symbol { get; set; }

    public decimal? RateToEur { get; set; }

    public bool? IsEnabled { get; set; }

    public int? SortOrder { get; set; }
}
