using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Configuration;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class TelegramEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Telegram.Webhook, async (
                Update update,
                ITelegramBotService telegramBotService,
                CancellationToken cancellationToken) =>
            {
                await telegramBotService.HandleUpdateAsync(update, cancellationToken);
                return Results.Ok();
            })
            .WithName("TelegramWebhook")
            .WithTags("Telegram")
            .ExcludeFromDescription();

        endpoints.MapPost(RouteConstants.Api.Telegram.GenerateToken, async (
                ITelegramBotService telegramBotService,
                IOptions<TelegramBotSettings> settings,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var token = await telegramBotService.GenerateLinkTokenAsync(userId, cancellationToken);
                return Results.Ok(new { token, botUsername = settings.Value.BotUsername });
            })
            .RequireAuthorization()
            .WithName("GenerateTelegramLinkToken")
            .WithTags("Telegram")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Telegram.Status, async (
                ITelegramBotService telegramBotService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isLinked = await telegramBotService.IsLinkedAsync(userId, cancellationToken);
                return Results.Ok(new { isLinked });
            })
            .RequireAuthorization()
            .WithName("GetTelegramStatus")
            .WithTags("Telegram")
            .Produces(200)
            .Produces(401);

        endpoints.MapDelete(RouteConstants.Api.Telegram.Unlink, async (
                ITelegramBotService telegramBotService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await telegramBotService.UnlinkAsync(userId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UnlinkTelegram")
            .WithTags("Telegram")
            .Produces(200)
            .Produces(401);
    }
}
