using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class ParsingSessionEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.ParsingSessions.GetAll, async (
                [AsParameters] ParsingSessionFilterDto filter,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetParsingSessionsAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetParsingSessions")
            .WithTags("Admin Parsing Sessions")
            .Produces<PagedResultDto<ParsingSessionDto>>();

        endpoints.MapGet(AdminRouteConstants.ParsingSessions.GetById, async (
                Guid id,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetParsingSessionByIdAsync(id, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetParsingSessionById")
            .WithTags("Admin Parsing Sessions")
            .Produces<ParsingSessionDetailDto>()
            .Produces(404);

        endpoints.MapPut(AdminRouteConstants.ParsingSessions.UpdateStatus, async (
                Guid id,
                UpdateParsingStatusDto request,
                IParsingSessionRepository parsingSessionRepository,
                CancellationToken cancellationToken) =>
            {
                var updated = await parsingSessionRepository.UpdateParsingStatus(id, request.ParsingStatus, cancellationToken);
                return updated ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateParsingSessionStatus")
            .WithTags("Admin Parsing Sessions")
            .Produces(204)
            .Produces(404);
    }
}
