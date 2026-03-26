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
                string? language,
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var languageCode = language ?? "en";
                var navItems = await context.NavigationItems
                    .AsNoTracking()
                    .Where(item => item.IsVisible)
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new
                    {
                        Title = item.Translations
                            .Where(t => t.LanguageCode == languageCode)
                            .Select(t => t.Title)
                            .FirstOrDefault()
                            ?? item.Translations
                                .Where(t => t.LanguageCode == "en")
                                .Select(t => t.Title)
                                .FirstOrDefault()
                            ?? string.Empty,
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
                string? language,
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var languageCode = language ?? "en";
                var articles = await context.NewsArticles
                    .AsNoTracking()
                    .Where(article => article.IsPublished)
                    .OrderBy(article => article.SortOrder)
                    .Select(article => new
                    {
                        article.Id,
                        Title = article.Translations
                            .Where(t => t.LanguageCode == languageCode)
                            .Select(t => t.Title)
                            .FirstOrDefault()
                            ?? article.Translations
                                .Where(t => t.LanguageCode == "en")
                                .Select(t => t.Title)
                                .FirstOrDefault()
                            ?? string.Empty,
                        Content = article.Translations
                            .Where(t => t.LanguageCode == languageCode)
                            .Select(t => t.Content)
                            .FirstOrDefault()
                            ?? article.Translations
                                .Where(t => t.LanguageCode == "en")
                                .Select(t => t.Content)
                                .FirstOrDefault()
                            ?? string.Empty,
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

        endpoints.MapGet(RouteConstants.Api.PublicConfig.Languages, async (
                CoreDataServiceDbContext context,
                CancellationToken cancellationToken) =>
            {
                var languages = await context.Languages
                    .AsNoTracking()
                    .Where(language => language.IsEnabled)
                    .OrderBy(language => language.SortOrder)
                    .Select(language => new
                    {
                        language.Code,
                        language.Name,
                        language.NativeName,
                        language.IsDefault,
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(languages);
            })
            .WithName("GetPublicLanguages")
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
