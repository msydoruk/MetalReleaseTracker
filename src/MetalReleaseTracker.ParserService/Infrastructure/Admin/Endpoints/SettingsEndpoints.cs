using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class SettingsEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        MapAiAgentEndpoints(endpoints);
        MapParsingSourceEndpoints(endpoints);
        MapCategorySettingsEndpoints(endpoints);
    }

    private static void MapAiAgentEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Settings.AiAgents.GetAll, async (
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.GetAiAgentsAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetAiAgents")
            .WithTags("Admin Settings")
            .Produces<List<AiAgentDto>>();

        endpoints.MapGet(AdminRouteConstants.Settings.AiAgents.GetById, async (
                Guid id,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.GetAiAgentByIdAsync(id, cancellationToken);
                return result == null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetAiAgentById")
            .WithTags("Admin Settings")
            .Produces<AiAgentDto>()
            .Produces(404);

        endpoints.MapPost(AdminRouteConstants.Settings.AiAgents.Create, async (
                CreateAiAgentDto request,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.CreateAiAgentAsync(request, cancellationToken);
                return Results.Created($"/api/admin/settings/ai-agents/{result.Id}", result);
            })
            .WithName("CreateAiAgent")
            .WithTags("Admin Settings")
            .Produces<AiAgentDto>(201);

        endpoints.MapPut(AdminRouteConstants.Settings.AiAgents.Update, async (
                Guid id,
                UpdateAiAgentDto request,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.UpdateAiAgentAsync(id, request, cancellationToken);
                return result == null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("UpdateAiAgent")
            .WithTags("Admin Settings")
            .Produces<AiAgentDto>()
            .Produces(404);

        endpoints.MapDelete(AdminRouteConstants.Settings.AiAgents.Delete, async (
                Guid id,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var deleted = await settingsService.DeleteAiAgentAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteAiAgent")
            .WithTags("Admin Settings")
            .Produces(204)
            .Produces(404);
    }

    private static void MapParsingSourceEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Settings.ParsingSources.GetAll, async (
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.GetParsingSourcesAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetParsingSources")
            .WithTags("Admin Settings")
            .Produces<List<ParsingSourceDto>>();

        endpoints.MapPut(AdminRouteConstants.Settings.ParsingSources.Update, async (
                Guid id,
                UpdateParsingSourceDto request,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.UpdateParsingSourceAsync(id, request, cancellationToken);
                return result == null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("UpdateParsingSource")
            .WithTags("Admin Settings")
            .Produces<ParsingSourceDto>()
            .Produces(404);
    }

    private static void MapCategorySettingsEndpoints(IEndpointRouteBuilder endpoints)
    {
        MapCategoryEndpoint(endpoints, "BandReference", AdminRouteConstants.Settings.BandReference);
        MapCategoryEndpoint(endpoints, "FlareSolverr", AdminRouteConstants.Settings.FlareSolverr);
        MapCategoryEndpoint(endpoints, "GeneralParser", AdminRouteConstants.Settings.GeneralParser);
    }

    private static void MapCategoryEndpoint(IEndpointRouteBuilder endpoints, string category, string route)
    {
        endpoints.MapGet(route, async (
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.GetSettingsByCategoryAsync(category, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"Get{category}Settings")
            .WithTags("Admin Settings")
            .Produces<CategorySettingsDto>();

        endpoints.MapPut(route, async (
                CategorySettingsDto request,
                ISettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.UpdateSettingsByCategoryAsync(category, request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"Update{category}Settings")
            .WithTags("Admin Settings")
            .Produces<CategorySettingsDto>();
    }
}
