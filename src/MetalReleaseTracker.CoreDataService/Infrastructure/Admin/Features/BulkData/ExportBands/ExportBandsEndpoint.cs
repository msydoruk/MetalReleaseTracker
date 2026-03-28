using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.BulkData.ExportBands;

public static class ExportBandsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.BulkData.ExportBands, async (
                string format,
                ExportBandsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (data, contentType, fileName) = await handler.HandleAsync(format, cancellationToken);
                return Results.File(data, contentType, fileName);
            })
            .WithName("AdminExportBands")
            .WithTags("Admin Bulk Data");
    }
}
