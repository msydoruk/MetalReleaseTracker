using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.CreateCurrency;

public static class CreateCurrencyEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Currencies.Create, async (
                CreateCurrencyRequest request,
                CreateCurrencyHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return Results.Created($"{AdminRouteConstants.Currencies.GetAll}/{result.Id}", result);
            })
            .WithName("AdminCreateCurrency")
            .WithTags("Admin Currencies")
            .Produces<CurrencyDto>(StatusCodes.Status201Created);
    }
}
