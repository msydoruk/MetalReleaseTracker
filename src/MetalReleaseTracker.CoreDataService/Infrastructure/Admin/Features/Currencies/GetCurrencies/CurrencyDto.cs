namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;

public class CurrencyDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public decimal RateToEur { get; set; }

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }
}
