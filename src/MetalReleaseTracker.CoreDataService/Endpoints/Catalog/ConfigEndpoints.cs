using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class ConfigEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.PublicConfig.Currencies, async (
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var currencies = await context.CurrencyRates
                    .AsNoTracking()
                    .Where(currency => currency.IsEnabled)
                    .OrderBy(currency => currency.SortOrder)
                    .Select(currency => new
                    {
                        currency.Code,
                        currency.Symbol,
                        currency.RateToEur,
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(currencies);
            })
            .WithName("GetPublicCurrencies")
            .WithTags("Config");

        endpoints.MapGet(RouteConstants.Api.PublicConfig.Navigation, async (
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var navItems = await context.NavigationItems
                    .AsNoTracking()
                    .Where(item => item.IsVisible)
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new
                    {
                        item.TitleEn,
                        item.TitleUa,
                        item.Path,
                        item.IconName,
                        item.IsProtected,
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(navItems);
            })
            .WithName("GetPublicNavigation")
            .WithTags("Config");

        endpoints.MapGet(RouteConstants.Api.PublicConfig.Translations, async (
                string language,
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var translations = await context.Translations
                    .AsNoTracking()
                    .Where(translation => translation.Language == language)
                    .ToDictionaryAsync(
                        translation => translation.Key,
                        translation => translation.Value,
                        cancellationToken);

                return Results.Ok(translations);
            })
            .WithName("GetPublicTranslations")
            .WithTags("Config");

        endpoints.MapGet(RouteConstants.Api.PublicConfig.News, async (
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var articles = await context.NewsArticles
                    .AsNoTracking()
                    .Where(article => article.IsPublished)
                    .OrderBy(article => article.SortOrder)
                    .Select(article => new
                    {
                        article.Id,
                        article.TitleEn,
                        article.TitleUa,
                        article.ContentEn,
                        article.ContentUa,
                        article.ChipLabel,
                        article.ChipColor,
                        article.IconName,
                        article.Date,
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(articles);
            })
            .WithName("GetPublicNews")
            .WithTags("Config");

        endpoints.MapGet(RouteConstants.Api.PublicConfig.SeoConfig, async (
                IAdminSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var settings = await settingsService.GetSettingsByCategoryAsync(
                    SettingCategories.Seo,
                    cancellationToken);

                return Results.Ok(settings.Settings);
            })
            .WithName("GetPublicSeoConfig")
            .WithTags("Config");
    }
}
