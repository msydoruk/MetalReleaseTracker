using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Services;

public class AdminSettingsSeedService : IAdminSettingsSeedService
{
    private readonly CoreDataServiceDbContext _context;
    private readonly ILogger<AdminSettingsSeedService> _logger;

    public AdminSettingsSeedService(CoreDataServiceDbContext context, ILogger<AdminSettingsSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedSettingsAsync(cancellationToken);
        await SeedCurrenciesAsync(cancellationToken);
        await SeedNavigationItemsAsync(cancellationToken);
        await SeedNewsArticlesAsync(cancellationToken);
    }

    private async Task SeedSettingsAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.Settings.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding default admin settings");

        var settings = new List<SettingEntity>
        {
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.JwtExpiresMinutes, "60"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.RefreshTokenExpirationDays, "7"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.PasswordMinLength, "8"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.PasswordRequireDigit, "true"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.PasswordRequireUppercase, "true"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.PasswordRequireLowercase, "true"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.PasswordRequireNonAlphanumeric, "true"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.LockoutTimeSpanMinutes, "15"),
            CreateSetting(SettingCategories.Authentication, SettingKeys.Authentication.MaxFailedAccessAttempts, "5"),

            CreateSetting(SettingCategories.Pagination, SettingKeys.Pagination.DefaultPageSize, "10"),
            CreateSetting(SettingCategories.Pagination, SettingKeys.Pagination.MaxPageSize, "100"),

            CreateSetting(SettingCategories.Storage, SettingKeys.Storage.PresignedUrlExpiryDays, "1"),

            CreateSetting(SettingCategories.FeatureToggles, SettingKeys.FeatureToggles.TelegramBotEnabled, "true"),
            CreateSetting(SettingCategories.FeatureToggles, SettingKeys.FeatureToggles.NotificationsEnabled, "true"),
            CreateSetting(SettingCategories.FeatureToggles, SettingKeys.FeatureToggles.ReviewsEnabled, "true"),
            CreateSetting(SettingCategories.FeatureToggles, SettingKeys.FeatureToggles.RegistrationEnabled, "true"),
            CreateSetting(SettingCategories.FeatureToggles, SettingKeys.FeatureToggles.GoogleAuthEnabled, "true"),

            CreateSetting(SettingCategories.Telegram, SettingKeys.Telegram.LinkTokenExpiryMinutes, "10"),

            CreateSetting(SettingCategories.Notifications, SettingKeys.Notifications.PriceDropEnabled, "true"),
            CreateSetting(SettingCategories.Notifications, SettingKeys.Notifications.BackInStockEnabled, "true"),
            CreateSetting(SettingCategories.Notifications, SettingKeys.Notifications.RestockEnabled, "true"),
            CreateSetting(SettingCategories.Notifications, SettingKeys.Notifications.NewVariantEnabled, "true"),
        };

        _context.Settings.AddRange(settings);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} default settings", settings.Count);
    }

    private async Task SeedCurrenciesAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.CurrencyRates.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding default currencies");

        var currencies = new List<CurrencyRateEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "EUR",
                Symbol = "\u20AC",
                RateToEur = 1.0m,
                IsEnabled = true,
                SortOrder = 1,
                UpdatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "UAH",
                Symbol = "\u20B4",
                RateToEur = 44.5m,
                IsEnabled = true,
                SortOrder = 2,
                UpdatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Symbol = "$",
                RateToEur = 1.08m,
                IsEnabled = true,
                SortOrder = 3,
                UpdatedAt = DateTime.UtcNow,
            },
        };

        _context.CurrencyRates.AddRange(currencies);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} currencies", currencies.Count);
    }

    private async Task SeedNavigationItemsAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.NavigationItems.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding default navigation items");

        var navItems = new List<NavigationItemEntity>
        {
            CreateNavItem("/", "Home", "\u0413\u043E\u043B\u043E\u0432\u043D\u0430", "HomeIcon", 1),
            CreateNavItem("/albums", "Albums", "\u0410\u043B\u044C\u0431\u043E\u043C\u0438", "AlbumIcon", 2),
            CreateNavItem("/bands", "Bands", "\u0413\u0443\u0440\u0442\u0438", "MusicNoteIcon", 3),
            CreateNavItem("/distributors", "Distributors", "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438", "StoreIcon", 4),
            CreateNavItem("/calendar", "Calendar", "\u041A\u0430\u043B\u0435\u043D\u0434\u0430\u0440", "CalendarMonthIcon", 5),
            CreateNavItem("/news", "News", "\u041D\u043E\u0432\u0438\u043D\u0438", "NewspaperIcon", 6),
            CreateNavItem("/about", "About", "\u041F\u0440\u043E \u043D\u0430\u0441", "InfoIcon", 7),
            CreateNavItem("/changelog", "Changelog", "\u0416\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D", "HistoryIcon", 8),
            CreateNavItem("/reviews", "Reviews", "\u0412\u0456\u0434\u0433\u0443\u043A\u0438", "RateReviewIcon", 9),
        };

        _context.NavigationItems.AddRange(navItems);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} navigation items", navItems.Count);
    }

    private async Task SeedNewsArticlesAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.NewsArticles.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding default news articles");

        var articles = new List<NewsArticleEntity>
        {
            CreateNewsArticle(
                "2 new distributors connected",
                "\u041F\u0456\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u043E 2 \u043D\u043E\u0432\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438",
                "We have added support for two new distributors: Werewolf (Poland) and Avantgarde Music / Sound Cave (Italy). The catalog now covers 9 distributors across Europe.",
                "\u0414\u043E\u0434\u0430\u043D\u043E \u043F\u0456\u0434\u0442\u0440\u0438\u043C\u043A\u0443 \u0434\u0432\u043E\u0445 \u043D\u043E\u0432\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432: Werewolf (\u041F\u043E\u043B\u044C\u0449\u0430) \u0442\u0430 Avantgarde Music / Sound Cave (\u0406\u0442\u0430\u043B\u0456\u044F). \u0422\u0435\u043F\u0435\u0440 \u043A\u0430\u0442\u0430\u043B\u043E\u0433 \u043E\u0445\u043E\u043F\u043B\u044E\u0454 9 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u043F\u043E \u0432\u0441\u0456\u0439 \u0404\u0432\u0440\u043E\u043F\u0456.",
                "New",
                "success",
                "StoreIcon",
                new DateTime(2026, 3, 17, 0, 0, 0, DateTimeKind.Utc),
                1),
            CreateNewsArticle(
                "Catalog changelog page added",
                "\u0414\u043E\u0434\u0430\u043D\u043E \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u044C \u043A\u0430\u0442\u0430\u043B\u043E\u0433\u0443",
                "You can now track all catalog changes in real time on the \"Changelog\" page: new releases, price updates, and removed items. Access it through the navigation menu.",
                "\u0422\u0435\u043F\u0435\u0440 \u0432\u0438 \u043C\u043E\u0436\u0435\u0442\u0435 \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0443\u0432\u0430\u0442\u0438 \u0432\u0441\u0456 \u0437\u043C\u0456\u043D\u0438 \u0432 \u043A\u0430\u0442\u0430\u043B\u043E\u0437\u0456 \u0432 \u0440\u0435\u0430\u043B\u044C\u043D\u043E\u043C\u0443 \u0447\u0430\u0441\u0456 \u043D\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u0446\u0456 \"\u0416\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D\": \u043D\u043E\u0432\u0456 \u0440\u0435\u043B\u0456\u0437\u0438, \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u043D\u044F \u0446\u0456\u043D \u0442\u0430 \u0432\u0438\u0434\u0430\u043B\u0435\u043D\u0456 \u043F\u043E\u0437\u0438\u0446\u0456\u0457. \u0421\u0442\u043E\u0440\u0456\u043D\u043A\u0430 \u0434\u043E\u0441\u0442\u0443\u043F\u043D\u0430 \u0447\u0435\u0440\u0435\u0437 \u043D\u0430\u0432\u0456\u0433\u0430\u0446\u0456\u0439\u043D\u0435 \u043C\u0435\u043D\u044E.",
                "New",
                "success",
                "TrackChangesIcon",
                new DateTime(2026, 3, 17, 0, 0, 0, DateTimeKind.Utc),
                2),
            CreateNewsArticle(
                "Favorites feature added",
                "\u0414\u043E\u0434\u0430\u043D\u043E \u0444\u0443\u043D\u043A\u0446\u0456\u043E\u043D\u0430\u043B \"\u0412\u0438\u0431\u0440\u0430\u043D\u0435\"",
                "You can now save your favorite albums! Sign in with Google, click the heart icon on any album card \u2014 and it will appear in your profile under the \"Favorites\" tab. We also added full-size cover image viewing and a feedback page.",
                "\u0422\u0435\u043F\u0435\u0440 \u0432\u0438 \u043C\u043E\u0436\u0435\u0442\u0435 \u0437\u0431\u0435\u0440\u0456\u0433\u0430\u0442\u0438 \u0443\u043B\u044E\u0431\u043B\u0435\u043D\u0456 \u0430\u043B\u044C\u0431\u043E\u043C\u0438! \u0423\u0432\u0456\u0439\u0434\u0456\u0442\u044C \u0447\u0435\u0440\u0435\u0437 Google, \u043D\u0430\u0442\u0438\u0441\u043D\u0456\u0442\u044C \u043D\u0430 \u0441\u0435\u0440\u0446\u0435 \u043D\u0430 \u043A\u0430\u0440\u0442\u0446\u0456 \u0430\u043B\u044C\u0431\u043E\u043C\u0443 \u2014 \u0456 \u0432\u0456\u043D \u0437'\u044F\u0432\u0438\u0442\u044C\u0441\u044F \u0443 \u0432\u0430\u0448\u043E\u043C\u0443 \u043A\u0430\u0431\u0456\u043D\u0435\u0442\u0456 \u043D\u0430 \u0432\u043A\u043B\u0430\u0434\u0446\u0456 \"\u0412\u0438\u0431\u0440\u0430\u043D\u0435\". \u0422\u0430\u043A\u043E\u0436 \u0434\u043E\u0434\u0430\u043D\u043E \u043F\u0435\u0440\u0435\u0433\u043B\u044F\u0434 \u043E\u0431\u043A\u043B\u0430\u0434\u0438\u043D\u043E\u043A \u0443 \u043F\u043E\u0432\u043D\u043E\u043C\u0443 \u0440\u043E\u0437\u043C\u0456\u0440\u0456 \u0442\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u0437\u0432\u043E\u0440\u043E\u0442\u043D\u043E\u0433\u043E \u0437\u0432'\u044F\u0437\u043A\u0443.",
                "New",
                "success",
                "FavoriteIcon",
                new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc),
                3),
            CreateNewsArticle(
                "New features planned",
                "\u041F\u043B\u0430\u043D\u0443\u044E\u0442\u044C\u0441\u044F \u043D\u043E\u0432\u0456 \u043C\u043E\u0436\u043B\u0438\u0432\u043E\u0441\u0442\u0456",
                "We are working on expanding functionality: ability to subscribe to price updates, new catalog items, and notifications about removed products. Stay tuned!",
                "\u041C\u0438 \u043F\u0440\u0430\u0446\u044E\u0454\u043C\u043E \u043D\u0430\u0434 \u0440\u043E\u0437\u0448\u0438\u0440\u0435\u043D\u043D\u044F\u043C \u0444\u0443\u043D\u043A\u0446\u0456\u043E\u043D\u0430\u043B\u0443: \u043C\u043E\u0436\u043B\u0438\u0432\u0456\u0441\u0442\u044C \u043F\u0456\u0434\u043F\u0438\u0441\u0430\u0442\u0438\u0441\u044F \u043D\u0430 \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u043D\u044F \u0446\u0456\u043D, \u043D\u043E\u0432\u0456 \u043F\u043E\u0437\u0438\u0446\u0456\u0457 \u0432 \u043A\u0430\u0442\u0430\u043B\u043E\u0437\u0456 \u0442\u0430 \u0441\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u043D\u044F \u043F\u0440\u043E \u0432\u0438\u0434\u0430\u043B\u0435\u043D\u0456 \u0442\u043E\u0432\u0430\u0440\u0438. \u0421\u043B\u0456\u0434\u043A\u0443\u0439\u0442\u0435 \u0437\u0430 \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u043D\u044F\u043C\u0438!",
                "Upcoming",
                "info",
                "RocketLaunchIcon",
                new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
                4),
            CreateNewsArticle(
                "Site is running in test mode",
                "\u0421\u0430\u0439\u0442 \u043F\u0440\u0430\u0446\u044E\u0454 \u0432 \u0442\u0435\u0441\u0442\u043E\u0432\u043E\u043C\u0443 \u0440\u0435\u0436\u0438\u043C\u0456",
                "Metal Release Tracker is currently running in test mode. Bugs and data inaccuracies are possible. If you find an issue, we appreciate your feedback.",
                "Metal Release Tracker \u043D\u0430\u0440\u0430\u0437\u0456 \u043F\u0440\u0430\u0446\u044E\u0454 \u0432 \u0442\u0435\u0441\u0442\u043E\u0432\u043E\u043C\u0443 \u0440\u0435\u0436\u0438\u043C\u0456. \u041C\u043E\u0436\u043B\u0438\u0432\u0456 \u0431\u0430\u0433\u0438 \u0442\u0430 \u043D\u0435\u0442\u043E\u0447\u043D\u043E\u0441\u0442\u0456 \u0432 \u0434\u0430\u043D\u0438\u0445. \u042F\u043A\u0449\u043E \u0432\u0438 \u0437\u043D\u0430\u0439\u0448\u043B\u0438 \u043F\u043E\u043C\u0438\u043B\u043A\u0443 - \u0431\u0443\u0434\u0435\u043C\u043E \u0432\u0434\u044F\u0447\u043D\u0456 \u0437\u0430 \u0437\u0432\u043E\u0440\u043E\u0442\u043D\u0438\u0439 \u0437\u0432'\u044F\u0437\u043E\u043A.",
                "Test Mode",
                "warning",
                "BuildIcon",
                new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
                5),
            CreateNewsArticle(
                "4 new distributors connected",
                "\u041F\u0456\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u043E 4 \u043D\u043E\u0432\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438",
                "We have added support for four new distributors: Napalm Records, Season of Mist, Paragon Records, and Black Metal Store. The catalog keeps growing - we now track 7 distributors across Europe.",
                "\u041C\u0438 \u0434\u043E\u0434\u0430\u043B\u0438 \u043F\u0456\u0434\u0442\u0440\u0438\u043C\u043A\u0443 \u0447\u043E\u0442\u0438\u0440\u044C\u043E\u0445 \u043D\u043E\u0432\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432: Napalm Records, Season of Mist, Paragon Records \u0442\u0430 Black Metal Store. \u0422\u0435\u043F\u0435\u0440 \u043A\u0430\u0442\u0430\u043B\u043E\u0433 \u0441\u0442\u0430\u0454 \u0449\u0435 \u0431\u0456\u043B\u044C\u0448\u0438\u043C - \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0443\u0454\u043C\u043E 7 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u043F\u043E \u0432\u0441\u0456\u0439 \u0404\u0432\u0440\u043E\u043F\u0456.",
                "New",
                "success",
                "NewReleasesIcon",
                new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                6),
        };

        _context.NewsArticles.AddRange(articles);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} news articles", articles.Count);
    }

    private static SettingEntity CreateSetting(string category, string key, string value)
    {
        return new SettingEntity
        {
            Key = key,
            Value = value,
            Category = category,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private static NavigationItemEntity CreateNavItem(string path, string titleEn, string titleUa, string iconName, int sortOrder)
    {
        return new NavigationItemEntity
        {
            Id = Guid.NewGuid(),
            TitleEn = titleEn,
            TitleUa = titleUa,
            Path = path,
            IconName = iconName,
            SortOrder = sortOrder,
            IsVisible = true,
            IsProtected = false,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private static NewsArticleEntity CreateNewsArticle(
        string titleEn,
        string titleUa,
        string contentEn,
        string contentUa,
        string chipLabel,
        string chipColor,
        string iconName,
        DateTime date,
        int sortOrder)
    {
        return new NewsArticleEntity
        {
            Id = Guid.NewGuid(),
            TitleEn = titleEn,
            TitleUa = titleUa,
            ContentEn = contentEn,
            ContentUa = contentUa,
            ChipLabel = chipLabel,
            ChipColor = chipColor,
            IconName = iconName,
            Date = date,
            SortOrder = sortOrder,
            IsPublished = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
