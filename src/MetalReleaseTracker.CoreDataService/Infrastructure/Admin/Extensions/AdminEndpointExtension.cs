using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AiSeo;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.DeleteAlbum;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.UpdateAlbum;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetAlbumsPerWeek;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetPopularGenres;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopDistributors;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopWatchedAlbums;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetUserGrowth;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AuditLog.GetAuditLogs;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.DeleteBand;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.CreateCurrency;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.UpdateCurrency;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Dashboard.GetDashboardStats;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetAlbumsMissingCovers;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingGenre;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingPhoto;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetDataQualitySummary;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetPotentialDuplicateBands;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.HideAlbum;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.CreateDistributor;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.DeleteDistributor;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributorById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.CreateLanguage;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.DeleteLanguage;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.UpdateLanguage;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.DeleteNavigationItem;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.UpdateNavigationItem;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.CreateNewsArticle;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.DeleteNewsArticle;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticleById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.UpdateNewsArticle;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.GetNotificationStats;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.SendBroadcast;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.DeleteReview;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Settings.GetSettings;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Settings.UpdateSettings;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetLinkedUsers;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetTelegramStats;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.BulkUpdateTranslations;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.UpdateTranslation;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUserById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.LockUser;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.UnlockUser;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.UpdateUserRole;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Extensions;

public static class AdminEndpointExtension
{
    public static WebApplication MapAdminEndpoints(this WebApplication app)
    {
        AdminAuthEndpoints.MapEndpoints(app);

        var adminGroup = app.MapGroup(string.Empty)
            .RequireAuthorization(AdminAuthExtension.AdminPolicy)
            .DisableAntiforgery();

        // Dashboard
        GetDashboardStatsEndpoint.MapEndpoint(adminGroup);

        // Settings
        GetSettingsEndpoint.MapEndpoint(adminGroup);
        UpdateSettingsEndpoint.MapEndpoint(adminGroup);

        // Distributors
        GetDistributorsEndpoint.MapEndpoint(adminGroup);
        GetDistributorByIdEndpoint.MapEndpoint(adminGroup);
        CreateDistributorEndpoint.MapEndpoint(adminGroup);
        UpdateDistributorEndpoint.MapEndpoint(adminGroup);
        DeleteDistributorEndpoint.MapEndpoint(adminGroup);

        // Bands
        GetBandsEndpoint.MapEndpoint(adminGroup);
        GetBandByIdEndpoint.MapEndpoint(adminGroup);
        UpdateBandEndpoint.MapEndpoint(adminGroup);
        MergeBandsEndpoint.MapEndpoint(adminGroup);
        DeleteBandEndpoint.MapEndpoint(adminGroup);

        // Albums
        GetAlbumsEndpoint.MapEndpoint(adminGroup);
        GetAlbumByIdEndpoint.MapEndpoint(adminGroup);
        UpdateAlbumEndpoint.MapEndpoint(adminGroup);
        DeleteAlbumEndpoint.MapEndpoint(adminGroup);
        BulkUpdateAlbumStatusEndpoint.MapEndpoint(adminGroup);

        // Currencies
        GetCurrenciesEndpoint.MapEndpoint(adminGroup);
        CreateCurrencyEndpoint.MapEndpoint(adminGroup);
        UpdateCurrencyEndpoint.MapEndpoint(adminGroup);

        // Navigation
        GetNavigationItemsEndpoint.MapEndpoint(adminGroup);
        CreateNavigationItemEndpoint.MapEndpoint(adminGroup);
        UpdateNavigationItemEndpoint.MapEndpoint(adminGroup);
        DeleteNavigationItemEndpoint.MapEndpoint(adminGroup);

        // Translations
        GetTranslationsEndpoint.MapEndpoint(adminGroup);
        UpdateTranslationEndpoint.MapEndpoint(adminGroup);
        BulkUpdateTranslationsEndpoint.MapEndpoint(adminGroup);

        // News
        GetNewsArticlesEndpoint.MapEndpoint(adminGroup);
        GetNewsArticleByIdEndpoint.MapEndpoint(adminGroup);
        CreateNewsArticleEndpoint.MapEndpoint(adminGroup);
        UpdateNewsArticleEndpoint.MapEndpoint(adminGroup);
        DeleteNewsArticleEndpoint.MapEndpoint(adminGroup);

        // Users
        GetUsersEndpoint.MapEndpoint(adminGroup);
        GetUserByIdEndpoint.MapEndpoint(adminGroup);
        UpdateUserRoleEndpoint.MapEndpoint(adminGroup);
        LockUserEndpoint.MapEndpoint(adminGroup);
        UnlockUserEndpoint.MapEndpoint(adminGroup);

        // Reviews
        GetReviewsEndpoint.MapEndpoint(adminGroup);
        DeleteReviewEndpoint.MapEndpoint(adminGroup);

        // Notifications
        GetNotificationStatsEndpoint.MapEndpoint(adminGroup);
        SendBroadcastEndpoint.MapEndpoint(adminGroup);

        // Telegram
        GetTelegramStatsEndpoint.MapEndpoint(adminGroup);
        GetLinkedUsersEndpoint.MapEndpoint(adminGroup);

        // Languages
        GetLanguagesEndpoint.MapEndpoint(adminGroup);
        CreateLanguageEndpoint.MapEndpoint(adminGroup);
        UpdateLanguageEndpoint.MapEndpoint(adminGroup);
        DeleteLanguageEndpoint.MapEndpoint(adminGroup);

        // Analytics
        GetAlbumsPerWeekEndpoint.MapEndpoint(adminGroup);
        GetUserGrowthEndpoint.MapEndpoint(adminGroup);
        GetPopularGenresEndpoint.MapEndpoint(adminGroup);
        GetTopDistributorsEndpoint.MapEndpoint(adminGroup);
        GetTopWatchedAlbumsEndpoint.MapEndpoint(adminGroup);

        // Audit Log
        GetAuditLogsEndpoint.MapEndpoint(adminGroup);

        // AI SEO
        AiSeoEndpoints.MapEndpoints(adminGroup);

        // Data Quality
        GetDataQualitySummaryEndpoint.MapEndpoint(adminGroup);
        GetAlbumsMissingCoversEndpoint.MapEndpoint(adminGroup);
        GetBandsMissingGenreEndpoint.MapEndpoint(adminGroup);
        GetBandsMissingPhotoEndpoint.MapEndpoint(adminGroup);
        GetPotentialDuplicateBandsEndpoint.MapEndpoint(adminGroup);
        HideAlbumEndpoint.MapEndpoint(adminGroup);

        return app;
    }
}
