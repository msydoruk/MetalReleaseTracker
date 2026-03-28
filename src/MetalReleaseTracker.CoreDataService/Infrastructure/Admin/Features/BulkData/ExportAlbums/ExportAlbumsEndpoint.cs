using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ExportAlbums;

public static class ExportAlbumsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.BulkData.ExportAlbums, async (
                string format,
                ExportAlbumsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (data, contentType, fileName) = await handler.HandleAsync(format, cancellationToken);
                return Results.File(data, contentType, fileName);
            })
            .WithName("AdminExportAlbums")
            .WithTags("Admin Bulk Data");
    }
}
