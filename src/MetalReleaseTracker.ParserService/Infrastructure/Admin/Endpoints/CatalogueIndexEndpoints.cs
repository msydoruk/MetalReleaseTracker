using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class CatalogueIndexEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.CatalogueIndex.GetAll, async (
                [AsParameters] CatalogueIndexFilterDto filter,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetCatalogueIndexAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCatalogueIndex")
            .WithTags("Admin Catalogue Index")
            .Produces<PagedResultDto<CatalogueIndexDto>>();

        endpoints.MapPut(AdminRouteConstants.CatalogueIndex.UpdateStatus, async (
                Guid id,
                UpdateStatusDto request,
                ICatalogueIndexRepository catalogueIndexRepository,
                ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
                CancellationToken cancellationToken) =>
            {
                await catalogueIndexRepository.UpdateStatusAsync(id, request.Status, cancellationToken);
                await ResetZeroPricePublicationStatusAsync(id, request.Status, catalogueIndexDetailRepository, cancellationToken);
                return Results.NoContent();
            })
            .WithName("UpdateCatalogueIndexStatus")
            .WithTags("Admin Catalogue Index")
            .Produces(204);

        endpoints.MapPut(AdminRouteConstants.CatalogueIndex.BatchUpdateStatus, async (
                BatchUpdateStatusDto request,
                ICatalogueIndexRepository catalogueIndexRepository,
                ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
                CancellationToken cancellationToken) =>
            {
                await catalogueIndexRepository.UpdateStatusBatchAsync(request.Ids, request.Status, cancellationToken);

                foreach (var id in request.Ids)
                {
                    await ResetZeroPricePublicationStatusAsync(id, request.Status, catalogueIndexDetailRepository, cancellationToken);
                }

                return Results.NoContent();
            })
            .WithName("BatchUpdateCatalogueIndexStatus")
            .WithTags("Admin Catalogue Index")
            .Produces(204);
    }

    private static async Task ResetZeroPricePublicationStatusAsync(
        Guid catalogueIndexId,
        CatalogueIndexStatus newStatus,
        ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
        CancellationToken cancellationToken)
    {
        if (newStatus != CatalogueIndexStatus.AiVerified)
        {
            return;
        }

        var detail = await catalogueIndexDetailRepository.GetByCatalogueIndexIdAsync(catalogueIndexId, cancellationToken);
        if (detail != null && detail.PublicationStatus == PublicationStatus.SkippedZeroPrice)
        {
            detail.PublicationStatus = PublicationStatus.Unpublished;
            await catalogueIndexDetailRepository.UpdateAsync(detail, cancellationToken);
        }
    }
}
