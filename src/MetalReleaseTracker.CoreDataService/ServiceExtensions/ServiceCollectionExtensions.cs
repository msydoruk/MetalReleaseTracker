using MetalReleaseTracker.CoreDataService.Data.MappingProfiles;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Configuration;
using MetalReleaseTracker.CoreDataService.Services.Implementation;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using Telegram.Bot;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddMinio();
            services.AddKafka(configuration);
            services.AddTelegramBot(configuration);
            services.AddEmailService(configuration);

            services.AddCommonServices()
                .AddRepositories()
                .AddDomainServices()
                .AddAuthServices();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IBandRepository, BandRepository>();
            services.AddScoped<IDistributorsRepository, DistributorRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
            services.AddScoped<IUserFollowedBandRepository, UserFollowedBandRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IAlbumChangeLogRepository, AlbumChangeLogRepository>();
            services.AddScoped<IAlbumRatingRepository, AlbumRatingRepository>();
            services.AddScoped<IUserAlbumWatchRepository, UserAlbumWatchRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<ITelegramLinkRepository, TelegramLinkRepository>();
            services.AddScoped<IEmailSubscriptionRepository, EmailSubscriptionRepository>();

            return services;
        }

        private static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<ITranslationResolverService, TranslationResolverService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IBandService, BandService>();
            services.AddScoped<IDistributorService, DistributorService>();
            services.AddScoped<IUserFavoriteService, UserFavoriteService>();
            services.AddScoped<IUserFollowedBandService, UserFollowedBandService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAlbumChangeLogService, AlbumChangeLogService>();
            services.AddScoped<IAlbumRatingService, AlbumRatingService>();
            services.AddScoped<ISeoMetaTagService, SeoMetaTagService>();
            services.AddScoped<ISitemapService, SitemapService>();
            services.AddScoped<IUserAlbumWatchService, UserAlbumWatchService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IImageUrlResolverService, ImageUrlResolverService>();
            services.AddScoped<ITelegramBotService, TelegramBotService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();

            return services;
        }

        private static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelegramBotSettings>(configuration.GetSection("TelegramBot"));
            var botToken = configuration["TelegramBot:BotToken"];
            if (!string.IsNullOrEmpty(botToken))
            {
                services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
            }

            return services;
        }

        private static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailServiceSettings>(configuration.GetSection("EmailService"));
            return services;
        }

        private static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<IFileStorageService, MinioFileStorageService>();
            return services;
        }
    }
}