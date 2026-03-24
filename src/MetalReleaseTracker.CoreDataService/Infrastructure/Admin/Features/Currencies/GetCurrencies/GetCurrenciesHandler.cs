using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;

public class GetCurrenciesHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetCurrenciesHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _context.CurrencyRates
            .AsNoTracking()
            .OrderBy(currency => currency.SortOrder)
            .Select(currency => new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Symbol = currency.Symbol,
                RateToEur = currency.RateToEur,
                IsEnabled = currency.IsEnabled,
                SortOrder = currency.SortOrder,
            })
            .ToListAsync(cancellationToken);

        return currencies;
    }
}
