using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.DeleteAlbum;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.UpdateAlbum;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.DeleteBand;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.CreateCurrency;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.GetCurrencies;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Currencies.UpdateCurrency;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Dashboard.GetDashboardStats;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.CreateDistributor;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.DeleteDistributor;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributorById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;
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
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Services;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Extensions;

public static class AdminServiceExtension
{
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminSettingsService, AdminSettingsService>();

        // Dashboard
        services.AddScoped<GetDashboardStatsHandler>();

        // Distributors
        services.AddScoped<GetDistributorsHandler>();
        services.AddScoped<GetDistributorByIdHandler>();
        services.AddScoped<CreateDistributorHandler>();
        services.AddScoped<UpdateDistributorHandler>();
        services.AddScoped<DeleteDistributorHandler>();

        // Bands
        services.AddScoped<GetBandsHandler>();
        services.AddScoped<GetBandByIdHandler>();
        services.AddScoped<UpdateBandHandler>();
        services.AddScoped<MergeBandsHandler>();
        services.AddScoped<DeleteBandHandler>();

        // Albums
        services.AddScoped<GetAlbumsHandler>();
        services.AddScoped<GetAlbumByIdHandler>();
        services.AddScoped<UpdateAlbumHandler>();
        services.AddScoped<DeleteAlbumHandler>();
        services.AddScoped<BulkUpdateAlbumStatusHandler>();

        // Currencies
        services.AddScoped<GetCurrenciesHandler>();
        services.AddScoped<CreateCurrencyHandler>();
        services.AddScoped<UpdateCurrencyHandler>();

        // Navigation
        services.AddScoped<GetNavigationItemsHandler>();
        services.AddScoped<CreateNavigationItemHandler>();
        services.AddScoped<UpdateNavigationItemHandler>();
        services.AddScoped<DeleteNavigationItemHandler>();

        // Translations
        services.AddScoped<GetTranslationsHandler>();
        services.AddScoped<UpdateTranslationHandler>();
        services.AddScoped<BulkUpdateTranslationsHandler>();

        // News
        services.AddScoped<GetNewsArticlesHandler>();
        services.AddScoped<GetNewsArticleByIdHandler>();
        services.AddScoped<CreateNewsArticleHandler>();
        services.AddScoped<UpdateNewsArticleHandler>();
        services.AddScoped<DeleteNewsArticleHandler>();

        // Users
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<UpdateUserRoleHandler>();
        services.AddScoped<LockUserHandler>();
        services.AddScoped<UnlockUserHandler>();

        // Reviews
        services.AddScoped<GetReviewsHandler>();
        services.AddScoped<DeleteReviewHandler>();

        // Notifications
        services.AddScoped<GetNotificationStatsHandler>();
        services.AddScoped<SendBroadcastHandler>();

        // Telegram
        services.AddScoped<GetTelegramStatsHandler>();
        services.AddScoped<GetLinkedUsersHandler>();

        // AI SEO
        services.AddScoped<IAiSeoService, AiSeoService>();

        return services;
    }
}
