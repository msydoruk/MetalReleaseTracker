namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.CreateCurrency;

public class CreateCurrencyRequest
{
    public string Code { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public decimal RateToEur { get; set; }

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }
}
