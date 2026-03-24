using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.UpdateCurrency;

public static class UpdateCurrencyEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Currencies.Update, async (
                Guid id,
                UpdateCurrencyRequest request,
                UpdateCurrencyHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("UpdateCurrency")
            .WithTags("Admin Currencies")
            .Produces<CurrencyDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
