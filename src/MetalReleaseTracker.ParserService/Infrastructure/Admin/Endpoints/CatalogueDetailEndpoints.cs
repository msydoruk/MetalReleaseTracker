using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class CatalogueDetailEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.CatalogueDetails.GetAll, async (
                [AsParameters] CatalogueDetailFilterDto filter,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetCatalogueDetailsAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCatalogueDetails")
            .WithTags("Admin Catalogue Details")
            .Produces<PagedResultDto<CatalogueDetailDto>>();
    }
}
