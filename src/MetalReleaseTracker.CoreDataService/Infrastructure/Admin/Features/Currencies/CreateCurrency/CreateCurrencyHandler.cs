using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.CreateCurrency;

public class CreateCurrencyHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateCurrencyHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyDto> HandleAsync(
        CreateCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new CurrencyRateEntity
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Symbol = request.Symbol,
            RateToEur = request.RateToEur,
            IsEnabled = request.IsEnabled,
            SortOrder = request.SortOrder,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.CurrencyRates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CurrencyDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Symbol = entity.Symbol,
            RateToEur = entity.RateToEur,
            IsEnabled = entity.IsEnabled,
            SortOrder = entity.SortOrder,
        };
    }
}
