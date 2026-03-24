using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.UpdateCurrency;

public class UpdateCurrencyHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateCurrencyHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyDto?> HandleAsync(
        Guid id,
        UpdateCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.CurrencyRates
            .FirstOrDefaultAsync(
                currency => currency.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.Code is not null)
        {
            entity.Code = request.Code;
        }

        if (request.Symbol is not null)
        {
            entity.Symbol = request.Symbol;
        }

        if (request.RateToEur.HasValue)
        {
            entity.RateToEur = request.RateToEur.Value;
        }

        if (request.IsEnabled.HasValue)
        {
            entity.IsEnabled = request.IsEnabled.Value;
        }

        if (request.SortOrder.HasValue)
        {
            entity.SortOrder = request.SortOrder.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
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
