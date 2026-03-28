using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ImportAlbums;

public static class ImportAlbumsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.BulkData.ImportAlbums, async (
                IFormFile file,
                bool confirm,
                ImportAlbumsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(file, confirm, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminImportAlbums")
            .WithTags("Admin Bulk Data")
            .Produces<ImportResult>()
            .DisableAntiforgery();
    }
}
