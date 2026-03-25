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
        await SeedTranslationsAsync(cancellationToken);
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

    private async Task SeedTranslationsAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.Translations.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding default translations");

        var translations = new List<TranslationEntity>();
        var now = DateTime.UtcNow;

        AddEnglishTranslations(translations, now);
        AddUkrainianTranslations(translations, now);

        _context.Translations.AddRange(translations);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} translations", translations.Count);
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

    private static void AddTranslation(List<TranslationEntity> list, string category, string key, string language, string value, DateTime now)
    {
        list.Add(new TranslationEntity
        {
            Id = Guid.NewGuid(),
            Key = key,
            Language = language,
            Value = value,
            Category = category,
            UpdatedAt = now,
        });
    }

    private static void AddEnglishTranslations(List<TranslationEntity> translations, DateTime now)
    {
        const string lang = "en";

        // nav
        AddTranslation(translations, "nav", "nav.home", lang, "Home", now);
        AddTranslation(translations, "nav", "nav.albums", lang, "Albums", now);
        AddTranslation(translations, "nav", "nav.bands", lang, "Bands", now);
        AddTranslation(translations, "nav", "nav.distributors", lang, "Distributors", now);
        AddTranslation(translations, "nav", "nav.calendar", lang, "Calendar", now);
        AddTranslation(translations, "nav", "nav.news", lang, "News", now);
        AddTranslation(translations, "nav", "nav.about", lang, "About", now);
        AddTranslation(translations, "nav", "nav.reviews", lang, "Reviews", now);
        AddTranslation(translations, "nav", "nav.changelog", lang, "Changelog", now);
        AddTranslation(translations, "nav", "nav.login", lang, "Login", now);
        AddTranslation(translations, "nav", "nav.signUp", lang, "Sign Up", now);
        AddTranslation(translations, "nav", "nav.profile", lang, "Profile", now);
        AddTranslation(translations, "nav", "nav.signOut", lang, "Sign Out", now);

        // home
        AddTranslation(translations, "home", "home.title", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "home", "home.subtitle", lang, "Ukrainian metal releases from foreign distributors and labels - all in one place", now);
        AddTranslation(translations, "home", "home.learnMore", lang, "Learn more about the project", now);

        // news
        AddTranslation(translations, "news", "news.title", lang, "News", now);
        AddTranslation(translations, "news", "news.subtitle", lang, "Latest updates and announcements", now);

        // header
        AddTranslation(translations, "header", "header.flagTooltip", lang, "Ukrainian metal releases sold by foreign distributors", now);
        AddTranslation(translations, "header", "header.currencyTooltip", lang, "Display currency", now);
        AddTranslation(translations, "header", "header.searchPlaceholder", lang, "Search bands & albums...", now);
        AddTranslation(translations, "header", "header.searchTooltip", lang, "Search", now);
        AddTranslation(translations, "header", "header.closeSearch", lang, "Close search", now);

        // albums
        AddTranslation(translations, "albums", "albums.heroTitle", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "albums", "albums.heroSubtitle", lang, "Ukrainian metal releases from foreign distributors and labels - all in one place. Find vinyl, CD, and tape releases and order directly from the source.", now);
        AddTranslation(translations, "albums", "albums.learnMore", lang, "Learn more about the project", now);
        AddTranslation(translations, "albums", "albums.metalReleases", lang, "Metal Releases", now);
        AddTranslation(translations, "albums", "albums.allDistributors", lang, "All", now);
        AddTranslation(translations, "albums", "albums.allDistributorsDropdown", lang, "All Distributors", now);
        AddTranslation(translations, "albums", "albums.filters", lang, "Filters", now);
        AddTranslation(translations, "albums", "albums.error", lang, "Failed to load albums. Please try again later.", now);
        AddTranslation(translations, "albums", "albums.noAlbums", lang, "No albums found matching your criteria.", now);
        AddTranslation(translations, "albums", "albums.tryAdjusting", lang, "Try adjusting your filters to see more results.", now);
        AddTranslation(translations, "albums", "albums.comparePrices", lang, "Group by album", now);
        AddTranslation(translations, "albums", "albums.searchPlaceholder", lang, "Search by band or album name...", now);
        AddTranslation(translations, "albums", "albums.suggestionBand", lang, "Band", now);
        AddTranslation(translations, "albums", "albums.suggestionAlbum", lang, "Album", now);

        // grouped
        AddTranslation(translations, "grouped", "grouped.stores", lang, "stores", now);
        AddTranslation(translations, "grouped", "grouped.moreStores", lang, "more", now);
        AddTranslation(translations, "grouped", "grouped.showLess", lang, "Show less", now);

        // albumCard
        AddTranslation(translations, "albumCard", "albumCard.statusNew", lang, "New", now);
        AddTranslation(translations, "albumCard", "albumCard.statusRestock", lang, "Restock", now);
        AddTranslation(translations, "albumCard", "albumCard.statusPreOrder", lang, "Pre-Order", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaCD", lang, "CD", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaVinyl", lang, "Vinyl", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaCassette", lang, "Cassette", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaUnknown", lang, "Unknown", now);
        AddTranslation(translations, "albumCard", "albumCard.viewInStore", lang, "Buy", now);

        // albumFilter
        AddTranslation(translations, "albumFilter", "albumFilter.filterAlbums", lang, "Filter Albums", now);
        AddTranslation(translations, "albumFilter", "albumFilter.resetFilters", lang, "Reset Filters", now);
        AddTranslation(translations, "albumFilter", "albumFilter.albumName", lang, "Album Name", now);
        AddTranslation(translations, "albumFilter", "albumFilter.searchPlaceholder", lang, "Search by album name...", now);
        AddTranslation(translations, "albumFilter", "albumFilter.from", lang, "From", now);
        AddTranslation(translations, "albumFilter", "albumFilter.to", lang, "To", now);
        AddTranslation(translations, "albumFilter", "albumFilter.mediaType", lang, "Media Type", now);
        AddTranslation(translations, "albumFilter", "albumFilter.all", lang, "All", now);
        AddTranslation(translations, "albumFilter", "albumFilter.cd", lang, "CD", now);
        AddTranslation(translations, "albumFilter", "albumFilter.vinyl", lang, "Vinyl", now);
        AddTranslation(translations, "albumFilter", "albumFilter.cassette", lang, "Cassette", now);
        AddTranslation(translations, "albumFilter", "albumFilter.status", lang, "Status", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusNew", lang, "New", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusRestock", lang, "Restock", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusPreorder", lang, "Preorder", now);
        AddTranslation(translations, "albumFilter", "albumFilter.band", lang, "Band", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allBands", lang, "All Bands", now);
        AddTranslation(translations, "albumFilter", "albumFilter.distributor", lang, "Distributor", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allDistributors", lang, "All Distributors", now);
        AddTranslation(translations, "albumFilter", "albumFilter.genre", lang, "Genre", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allGenres", lang, "All Genres", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortBy", lang, "Sort By", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortDate", lang, "Date", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortName", lang, "Name", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortPrice", lang, "Price", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortBand", lang, "Band", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortYear", lang, "Year", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortDistributor", lang, "Distributor", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortStores", lang, "Stores", now);
        AddTranslation(translations, "albumFilter", "albumFilter.desc", lang, "Desc", now);
        AddTranslation(translations, "albumFilter", "albumFilter.asc", lang, "Asc", now);
        AddTranslation(translations, "albumFilter", "albumFilter.yearRange", lang, "Year Range", now);
        AddTranslation(translations, "albumFilter", "albumFilter.minYear", lang, "From", now);
        AddTranslation(translations, "albumFilter", "albumFilter.maxYear", lang, "To", now);
        AddTranslation(translations, "albumFilter", "albumFilter.priceRange", lang, "Price Range", now);
        AddTranslation(translations, "albumFilter", "albumFilter.applyFilters", lang, "Apply Filters", now);

        // pagination
        AddTranslation(translations, "pagination", "pagination.showing", lang, "Showing {start}-{end} of {total} items", now);
        AddTranslation(translations, "pagination", "pagination.itemsPerPage", lang, "Items per page:", now);
        AddTranslation(translations, "pagination", "pagination.goToFirstPage", lang, "Go to first page", now);
        AddTranslation(translations, "pagination", "pagination.goToLastPage", lang, "Go to last page", now);
        AddTranslation(translations, "pagination", "pagination.goToNextPage", lang, "Go to next page", now);
        AddTranslation(translations, "pagination", "pagination.goToPreviousPage", lang, "Go to previous page", now);
        AddTranslation(translations, "pagination", "pagination.goToPage", lang, "Go to page", now);

        // distributors
        AddTranslation(translations, "distributors", "distributors.title", lang, "Metal Distributors", now);
        AddTranslation(translations, "distributors", "distributors.subtitle", lang, "Browse our collection of metal music distributors and shops", now);
        AddTranslation(translations, "distributors", "distributors.browseProducts", lang, "Browse Products", now);
        AddTranslation(translations, "distributors", "distributors.website", lang, "Website", now);
        AddTranslation(translations, "distributors", "distributors.products", lang, "Products", now);
        AddTranslation(translations, "distributors", "distributors.noDistributors", lang, "No distributors found.", now);
        AddTranslation(translations, "distributors", "distributors.checkBack", lang, "Please check back later for updates.", now);
        AddTranslation(translations, "distributors", "distributors.error", lang, "Failed to load distributors. Please try again later.", now);
        AddTranslation(translations, "distributors", "distributors.fallbackDescription", lang, "Metal music distributor and shop.", now);

        // bands
        AddTranslation(translations, "bands", "bands.title", lang, "Ukrainian Metal Bands", now);
        AddTranslation(translations, "bands", "bands.subtitle", lang, "Browse bands from the Ukrainian metal scene", now);
        AddTranslation(translations, "bands", "bands.album", lang, "album", now);
        AddTranslation(translations, "bands", "bands.albums", lang, "albums", now);
        AddTranslation(translations, "bands", "bands.browseAlbums", lang, "Browse Albums", now);
        AddTranslation(translations, "bands", "bands.noBands", lang, "No bands found.", now);
        AddTranslation(translations, "bands", "bands.checkBack", lang, "Please check back later for updates.", now);
        AddTranslation(translations, "bands", "bands.error", lang, "Failed to load bands. Please try again later.", now);
        AddTranslation(translations, "bands", "bands.noDescription", lang, "No description available.", now);
        AddTranslation(translations, "bands", "bands.viewAlbums", lang, "View Albums", now);
        AddTranslation(translations, "bands", "bands.heading", lang, "Metal Bands", now);
        AddTranslation(translations, "bands", "bands.headingSubtitle", lang, "Browse our collection of metal bands and discover their releases. The album count shown reflects only releases available from foreign distributors tracked by this hub, not the band's full discography.", now);
        AddTranslation(translations, "bands", "bands.searchPlaceholder", lang, "Search by band name...", now);
        AddTranslation(translations, "bands", "bands.noResults", lang, "No bands found matching your search.", now);

        // collection
        AddTranslation(translations, "collection", "collection.favorite", lang, "Favorite", now);
        AddTranslation(translations, "collection", "collection.want", lang, "Want", now);
        AddTranslation(translations, "collection", "collection.owned", lang, "Owned", now);
        AddTranslation(translations, "collection", "collection.remove", lang, "Remove", now);

        // profile
        AddTranslation(translations, "profile", "profile.loading", lang, "Loading profile...", now);
        AddTranslation(translations, "profile", "profile.authInfo", lang, "Authentication Information", now);
        AddTranslation(translations, "profile", "profile.loginTime", lang, "Login Time", now);
        AddTranslation(translations, "profile", "profile.sessionValidUntil", lang, "Session Valid Until", now);
        AddTranslation(translations, "profile", "profile.signOut", lang, "Sign Out", now);
        AddTranslation(translations, "profile", "profile.userInfo", lang, "User Information", now);
        AddTranslation(translations, "profile", "profile.userId", lang, "User ID", now);
        AddTranslation(translations, "profile", "profile.email", lang, "Email", now);
        AddTranslation(translations, "profile", "profile.username", lang, "Username", now);
        AddTranslation(translations, "profile", "profile.emailNotProvided", lang, "Email not provided", now);
        AddTranslation(translations, "profile", "profile.favorites", lang, "Favorites", now);
        AddTranslation(translations, "profile", "profile.wishlist", lang, "Wishlist", now);
        AddTranslation(translations, "profile", "profile.collection", lang, "Collection", now);
        AddTranslation(translations, "profile", "profile.profileTab", lang, "Profile", now);
        AddTranslation(translations, "profile", "profile.exportCsv", lang, "Export CSV", now);

        // reviews
        AddTranslation(translations, "reviews", "reviews.title", lang, "Reviews", now);
        AddTranslation(translations, "reviews", "reviews.subtitle", lang, "Share your experience with Metal Release Tracker", now);
        AddTranslation(translations, "reviews", "reviews.messageLabel", lang, "Your review", now);
        AddTranslation(translations, "reviews", "reviews.messagePlaceholder", lang, "Share your thoughts about the project...", now);
        AddTranslation(translations, "reviews", "reviews.submit", lang, "Submit", now);
        AddTranslation(translations, "reviews", "reviews.sending", lang, "Submitting...", now);
        AddTranslation(translations, "reviews", "reviews.success", lang, "Thank you! Your review has been published.", now);
        AddTranslation(translations, "reviews", "reviews.error", lang, "Failed to submit review. Please try again later.", now);
        AddTranslation(translations, "reviews", "reviews.loginRequired", lang, "Log in to leave a review.", now);
        AddTranslation(translations, "reviews", "reviews.listTitle", lang, "User Reviews", now);
        AddTranslation(translations, "reviews", "reviews.empty", lang, "No reviews yet. Be the first to share your thoughts!", now);

        // changelog
        AddTranslation(translations, "changelog", "changelog.title", lang, "Changelog", now);
        AddTranslation(translations, "changelog", "changelog.subtitle", lang, "History of album synchronization events across all distributors", now);
        AddTranslation(translations, "changelog", "changelog.date", lang, "Date", now);
        AddTranslation(translations, "changelog", "changelog.band", lang, "Band", now);
        AddTranslation(translations, "changelog", "changelog.album", lang, "Album", now);
        AddTranslation(translations, "changelog", "changelog.price", lang, "Price", now);
        AddTranslation(translations, "changelog", "changelog.distributor", lang, "Distributor", now);
        AddTranslation(translations, "changelog", "changelog.status", lang, "Status", now);
        AddTranslation(translations, "changelog", "changelog.statusNew", lang, "New", now);
        AddTranslation(translations, "changelog", "changelog.statusUpdated", lang, "Updated", now);
        AddTranslation(translations, "changelog", "changelog.statusDeleted", lang, "Deleted", now);
        AddTranslation(translations, "changelog", "changelog.empty", lang, "No changelog entries yet", now);
        AddTranslation(translations, "changelog", "changelog.emptyHint", lang, "Changelog entries will appear here as albums are synchronized.", now);
        AddTranslation(translations, "changelog", "changelog.error", lang, "Failed to load changelog. Please try again later.", now);

        // favorites
        AddTranslation(translations, "favorites", "favorites.empty", lang, "No favorites yet", now);
        AddTranslation(translations, "favorites", "favorites.emptyHint", lang, "Click the heart icon on album cards to add them to your favorites.", now);

        // login
        AddTranslation(translations, "login", "login.title", lang, "Log In", now);
        AddTranslation(translations, "login", "login.or", lang, "OR", now);
        AddTranslation(translations, "login", "login.emailLabel", lang, "Email Address", now);
        AddTranslation(translations, "login", "login.passwordLabel", lang, "Password", now);
        AddTranslation(translations, "login", "login.rememberMe", lang, "Remember me", now);
        AddTranslation(translations, "login", "login.signIn", lang, "Sign In", now);
        AddTranslation(translations, "login", "login.noAccount", lang, "Don't have an account?", now);
        AddTranslation(translations, "login", "login.signUp", lang, "Sign Up", now);
        AddTranslation(translations, "login", "login.validation", lang, "Email and password are required", now);

        // register
        AddTranslation(translations, "register", "register.title", lang, "Sign Up", now);
        AddTranslation(translations, "register", "register.or", lang, "OR", now);
        AddTranslation(translations, "register", "register.emailLabel", lang, "Email Address", now);
        AddTranslation(translations, "register", "register.displayName", lang, "Display Name (optional)", now);
        AddTranslation(translations, "register", "register.displayNameHelper", lang, "If left empty, your email will be used", now);
        AddTranslation(translations, "register", "register.passwordLabel", lang, "Password", now);
        AddTranslation(translations, "register", "register.confirmPassword", lang, "Confirm Password", now);
        AddTranslation(translations, "register", "register.submit", lang, "Register", now);
        AddTranslation(translations, "register", "register.hasAccount", lang, "Already have an account?", now);
        AddTranslation(translations, "register", "register.signIn", lang, "Sign In", now);
        AddTranslation(translations, "register", "register.validationRequired", lang, "Email, password, and password confirmation are required", now);
        AddTranslation(translations, "register", "register.validationMismatch", lang, "Passwords do not match", now);

        // about
        AddTranslation(translations, "about", "about.title", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "about", "about.heroSubtitle", lang, "The centralized hub for tracking Ukrainian metal releases sold by foreign distributors and labels.", now);
        AddTranslation(translations, "about", "about.problemTitle", lang, "The Problem", now);
        AddTranslation(translations, "about", "about.problemText", lang, "Ukrainian metal bands are releasing incredible music - but their physical releases (vinyl, CD, tape) are often distributed exclusively through foreign labels and distros scattered across Europe. For fans in Ukraine and worldwide, finding where to buy these releases means manually checking dozens of online shops, many of which have no search filters for Ukrainian bands. Releases come and go, and by the time you find out about one, it's often sold out.", now);
        AddTranslation(translations, "about", "about.solutionTitle", lang, "The Solution", now);
        AddTranslation(translations, "about", "about.solutionText", lang, "Metal Release Tracker automatically scans the catalogs of foreign distributors and labels, extracts every Ukrainian metal release it finds, and presents them in a single, searchable catalog. Each release links directly to the distributor's product page so you can order it immediately. New releases, restocks, and pre-orders are tracked continuously - so you'll always know what's available and where to get it.", now);
        AddTranslation(translations, "about", "about.howItWorks", lang, "How It Works", now);
        AddTranslation(translations, "about", "about.features.discover.title", lang, "Discover", now);
        AddTranslation(translations, "about", "about.features.discover.description", lang, "Find Ukrainian metal releases available from foreign distributors and labels that are otherwise hard to track down.", now);
        AddTranslation(translations, "about", "about.features.globalReach.title", lang, "Global Reach", now);
        AddTranslation(translations, "about", "about.features.globalReach.description", lang, "We aggregate catalogs from distributors and labels across Europe and beyond, bringing them all to one place.", now);
        AddTranslation(translations, "about", "about.features.orderDirect.title", lang, "Order Direct", now);
        AddTranslation(translations, "about", "about.features.orderDirect.description", lang, "Go straight to the distributor's store page and order physical releases directly from the source - no middlemen.", now);
        AddTranslation(translations, "about", "about.features.stayUpdated.title", lang, "Stay Updated", now);
        AddTranslation(translations, "about", "about.features.stayUpdated.description", lang, "Our automated parsers continuously scan distributor catalogs so you never miss a new release, restock, or pre-order.", now);
        AddTranslation(translations, "about", "about.features.allFormats.title", lang, "All Formats", now);
        AddTranslation(translations, "about", "about.features.allFormats.description", lang, "Vinyl, CD, cassette - browse releases across all physical media formats in one unified catalog.", now);
        AddTranslation(translations, "about", "about.features.forCommunity.title", lang, "For the Community", now);
        AddTranslation(translations, "about", "about.features.forCommunity.description", lang, "Built by Ukrainian metalheads, for Ukrainian metalheads. Supporting the scene by making its music more accessible worldwide.", now);
        AddTranslation(translations, "about", "about.networkTitle", lang, "Growing Network of Distributors", now);
        AddTranslation(translations, "about", "about.networkText", lang, "We're continuously adding new foreign distributors and labels that carry Ukrainian metal releases. Our automated system monitors their catalogs around the clock, ensuring the most up-to-date information on availability, pricing, and new arrivals.", now);
        AddTranslation(translations, "about", "about.supportTitle", lang, "Support Ukrainian Metal", now);
        AddTranslation(translations, "about", "about.supportText", lang, "Every purchase from a legitimate distributor supports Ukrainian artists and the global metal community. Browse the catalog, find something heavy, and order it directly.", now);

        // pageMeta
        AddTranslation(translations, "pageMeta", "pageMeta.homeTitle", lang, "Ukrainian Metal Releases from Foreign Distributors", now);
        AddTranslation(translations, "pageMeta", "pageMeta.homeDescription", lang, "Browse Ukrainian metal releases available from foreign distributors. Filter by band, format, price. Vinyl, CD, cassette - order directly from the source.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.albumsTitle", lang, "Albums - Ukrainian Metal Releases", now);
        AddTranslation(translations, "pageMeta", "pageMeta.albumsDescription", lang, "Browse Ukrainian metal releases available from foreign distributors. Filter by band, format, price. Vinyl, CD, cassette - order directly from the source.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.bandsTitle", lang, "Bands - Ukrainian Metal Bands", now);
        AddTranslation(translations, "pageMeta", "pageMeta.bandsDescription", lang, "Explore Ukrainian metal bands whose physical releases are sold by foreign distributors and labels worldwide.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.distributorsTitle", lang, "Distributors - Foreign Metal Labels & Shops", now);
        AddTranslation(translations, "pageMeta", "pageMeta.distributorsDescription", lang, "Foreign distributors and labels selling Ukrainian metal releases. Osmose Productions, Drakkar, Black Metal Vendor and more.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.newsTitle", lang, "News", now);
        AddTranslation(translations, "pageMeta", "pageMeta.newsDescription", lang, "Latest news and updates from Metal Release Tracker.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.aboutTitle", lang, "About", now);
        AddTranslation(translations, "pageMeta", "pageMeta.aboutDescription", lang, "Metal Release Tracker aggregates Ukrainian metal releases from foreign distributors and labels into one searchable catalog.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.reviewsTitle", lang, "Reviews", now);
        AddTranslation(translations, "pageMeta", "pageMeta.reviewsDescription", lang, "Read and share reviews about Metal Release Tracker.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.loginTitle", lang, "Log In", now);
        AddTranslation(translations, "pageMeta", "pageMeta.registerTitle", lang, "Sign Up", now);
        AddTranslation(translations, "pageMeta", "pageMeta.changelogTitle", lang, "Changelog", now);
        AddTranslation(translations, "pageMeta", "pageMeta.changelogDescription", lang, "History of album synchronization events. Track new, updated, and deleted releases across all distributors.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.notFoundTitle", lang, "Page Not Found", now);

        // newArrivals
        AddTranslation(translations, "newArrivals", "newArrivals.title", lang, "New Arrivals", now);
        AddTranslation(translations, "newArrivals", "newArrivals.subtitle", lang, "Added in the last 14 days", now);
        AddTranslation(translations, "newArrivals", "newArrivals.viewAll", lang, "View all new arrivals", now);

        // recentlyViewed
        AddTranslation(translations, "recentlyViewed", "recentlyViewed.title", lang, "Recently Viewed", now);

        // bandDetail
        AddTranslation(translations, "bandDetail", "bandDetail.albumsBy", lang, "Releases by {bandName}", now);
        AddTranslation(translations, "bandDetail", "bandDetail.noAlbums", lang, "No releases found for this band.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.backToBands", lang, "Back to Bands", now);
        AddTranslation(translations, "bandDetail", "bandDetail.error", lang, "Failed to load band details. Please try again later.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.notFound", lang, "Band not found.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.similarBands", lang, "Similar Bands", now);
        AddTranslation(translations, "bandDetail", "bandDetail.follow", lang, "Follow", now);
        AddTranslation(translations, "bandDetail", "bandDetail.following", lang, "Following", now);
        AddTranslation(translations, "bandDetail", "bandDetail.follower", lang, "follower", now);
        AddTranslation(translations, "bandDetail", "bandDetail.followers", lang, "followers", now);
        AddTranslation(translations, "bandDetail", "bandDetail.viewOnMetalArchives", lang, "View on Metal Archives", now);
        AddTranslation(translations, "bandDetail", "bandDetail.formedIn", lang, "Formed in", now);

        // albumDetail
        AddTranslation(translations, "albumDetail", "albumDetail.backToAlbums", lang, "Back to Albums", now);
        AddTranslation(translations, "albumDetail", "albumDetail.availableAt", lang, "Available at {count} stores", now);
        AddTranslation(translations, "albumDetail", "albumDetail.availableAtOne", lang, "Available at 1 store", now);
        AddTranslation(translations, "albumDetail", "albumDetail.buy", lang, "Buy", now);
        AddTranslation(translations, "albumDetail", "albumDetail.moreByBand", lang, "More releases by {bandName}", now);
        AddTranslation(translations, "albumDetail", "albumDetail.notFound", lang, "Album not found.", now);
        AddTranslation(translations, "albumDetail", "albumDetail.error", lang, "Failed to load album details.", now);
        AddTranslation(translations, "albumDetail", "albumDetail.label", lang, "Label", now);
        AddTranslation(translations, "albumDetail", "albumDetail.press", lang, "Press", now);
        AddTranslation(translations, "albumDetail", "albumDetail.shipsFrom", lang, "Ships from", now);
        AddTranslation(translations, "albumDetail", "albumDetail.listenOnBandcamp", lang, "Listen on Bandcamp", now);
        AddTranslation(translations, "albumDetail", "albumDetail.alsoAvailableAs", lang, "Also available as", now);

        // priceHistory
        AddTranslation(translations, "priceHistory", "priceHistory.title", lang, "Price History", now);
        AddTranslation(translations, "priceHistory", "priceHistory.noData", lang, "No price history available", now);

        // rating
        AddTranslation(translations, "rating", "rating.average", lang, "{count} ratings", now);
        AddTranslation(translations, "rating", "rating.averageOne", lang, "1 rating", now);
        AddTranslation(translations, "rating", "rating.loginToRate", lang, "Log in to rate", now);
        AddTranslation(translations, "rating", "rating.yourRating", lang, "Your rating", now);

        // notFound
        AddTranslation(translations, "notFound", "notFound.title", lang, "404", now);
        AddTranslation(translations, "notFound", "notFound.heading", lang, "Page Not Found", now);
        AddTranslation(translations, "notFound", "notFound.message", lang, "The page you are looking for doesn't exist or has been moved.", now);
        AddTranslation(translations, "notFound", "notFound.backHome", lang, "Go to Home", now);
        AddTranslation(translations, "notFound", "notFound.backAlbums", lang, "Browse Albums", now);

        // footer
        AddTranslation(translations, "footer", "footer.rights", lang, "All rights reserved.", now);
        AddTranslation(translations, "footer", "footer.suggestDistributor", lang, "Suggest a Distributor", now);

        // notifications
        AddTranslation(translations, "notifications", "notifications.title", lang, "Notifications", now);
        AddTranslation(translations, "notifications", "notifications.noNotifications", lang, "No notifications yet", now);
        AddTranslation(translations, "notifications", "notifications.markAllRead", lang, "Mark all as read", now);

        // watch
        AddTranslation(translations, "watch", "watch.watchTooltip", lang, "Get notified about price drops and restocks", now);
        AddTranslation(translations, "watch", "watch.unwatchTooltip", lang, "Stop watching this album", now);

        // calendar
        AddTranslation(translations, "calendar", "calendar.title", lang, "Release Calendar", now);
        AddTranslation(translations, "calendar", "calendar.description", lang, "Upcoming pre-orders and recent releases from Ukrainian metal bands.", now);
        AddTranslation(translations, "calendar", "calendar.preOrders", lang, "Pre-Orders", now);
        AddTranslation(translations, "calendar", "calendar.recentReleases", lang, "Recent Releases (Last 30 Days)", now);
        AddTranslation(translations, "calendar", "calendar.noReleases", lang, "No releases found matching your filters.", now);
        AddTranslation(translations, "calendar", "calendar.preOrder", lang, "Pre-Order", now);

        // telegram
        AddTranslation(translations, "telegram", "telegram.title", lang, "Telegram Notifications", now);
        AddTranslation(translations, "telegram", "telegram.description", lang, "Link your Telegram account to receive instant notifications about price drops, restocks, and new releases.", now);
        AddTranslation(translations, "telegram", "telegram.linked", lang, "Telegram account linked", now);
        AddTranslation(translations, "telegram", "telegram.unlink", lang, "Unlink", now);
        AddTranslation(translations, "telegram", "telegram.linkButton", lang, "Link Telegram", now);
        AddTranslation(translations, "telegram", "telegram.sendCommand", lang, "Send this command to the bot:", now);
        AddTranslation(translations, "telegram", "telegram.copy", lang, "Copy", now);
        AddTranslation(translations, "telegram", "telegram.openBot", lang, "Open Bot", now);
    }

    private static void AddUkrainianTranslations(List<TranslationEntity> translations, DateTime now)
    {
        const string lang = "ua";

        // nav
        AddTranslation(translations, "nav", "nav.home", lang, "\u0413\u043E\u043B\u043E\u0432\u043D\u0430", now);
        AddTranslation(translations, "nav", "nav.albums", lang, "\u0410\u043B\u044C\u0431\u043E\u043C\u0438", now);
        AddTranslation(translations, "nav", "nav.bands", lang, "\u0413\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "nav", "nav.distributors", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438", now);
        AddTranslation(translations, "nav", "nav.calendar", lang, "\u041A\u0430\u043B\u0435\u043D\u0434\u0430\u0440", now);
        AddTranslation(translations, "nav", "nav.news", lang, "\u041D\u043E\u0432\u0438\u043D\u0438", now);
        AddTranslation(translations, "nav", "nav.about", lang, "\u041F\u0440\u043E \u043F\u0440\u043E\u0454\u043A\u0442", now);
        AddTranslation(translations, "nav", "nav.reviews", lang, "\u0412\u0456\u0434\u0433\u0443\u043A\u0438", now);
        AddTranslation(translations, "nav", "nav.changelog", lang, "\u0416\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D", now);
        AddTranslation(translations, "nav", "nav.login", lang, "\u0423\u0432\u0456\u0439\u0442\u0438", now);
        AddTranslation(translations, "nav", "nav.signUp", lang, "\u0420\u0435\u0454\u0441\u0442\u0440\u0430\u0446\u0456\u044F", now);
        AddTranslation(translations, "nav", "nav.profile", lang, "\u041F\u0440\u043E\u0444\u0456\u043B\u044C", now);
        AddTranslation(translations, "nav", "nav.signOut", lang, "\u0412\u0438\u0439\u0442\u0438", now);

        // home
        AddTranslation(translations, "home", "home.title", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "home", "home.subtitle", lang, "\u0420\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432 - \u0432\u0441\u0435 \u0432 \u043E\u0434\u043D\u043E\u043C\u0443 \u043C\u0456\u0441\u0446\u0456", now);
        AddTranslation(translations, "home", "home.learnMore", lang, "\u0414\u0456\u0437\u043D\u0430\u0442\u0438\u0441\u044F \u0431\u0456\u043B\u044C\u0448\u0435 \u043F\u0440\u043E \u043F\u0440\u043E\u0454\u043A\u0442", now);

        // news
        AddTranslation(translations, "news", "news.title", lang, "\u041D\u043E\u0432\u0438\u043D\u0438", now);
        AddTranslation(translations, "news", "news.subtitle", lang, "\u041E\u0441\u0442\u0430\u043D\u043D\u0456 \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u043D\u044F \u0442\u0430 \u0430\u043D\u043E\u043D\u0441\u0438", now);

        // header
        AddTranslation(translations, "header", "header.flagTooltip", lang, "\u0420\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432", now);
        AddTranslation(translations, "header", "header.currencyTooltip", lang, "\u0412\u0430\u043B\u044E\u0442\u0430 \u0432\u0456\u0434\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "header", "header.searchPlaceholder", lang, "\u041F\u043E\u0448\u0443\u043A \u0433\u0443\u0440\u0442\u0456\u0432 \u0442\u0430 \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432...", now);
        AddTranslation(translations, "header", "header.searchTooltip", lang, "\u041F\u043E\u0448\u0443\u043A", now);
        AddTranslation(translations, "header", "header.closeSearch", lang, "\u0417\u0430\u043A\u0440\u0438\u0442\u0438 \u043F\u043E\u0448\u0443\u043A", now);

        // albums
        AddTranslation(translations, "albums", "albums.heroTitle", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "albums", "albums.heroSubtitle", lang, "\u0420\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432 - \u0432\u0441\u0435 \u0432 \u043E\u0434\u043D\u043E\u043C\u0443 \u043C\u0456\u0441\u0446\u0456. \u0417\u043D\u0430\u0445\u043E\u0434\u044C\u0442\u0435 \u0432\u0456\u043D\u0456\u043B, CD \u0442\u0430 \u043A\u0430\u0441\u0435\u0442\u0438 \u0456 \u0437\u0430\u043C\u043E\u0432\u043B\u044F\u0439\u0442\u0435 \u043D\u0430\u043F\u0440\u044F\u043C\u0443.", now);
        AddTranslation(translations, "albums", "albums.learnMore", lang, "\u0414\u0456\u0437\u043D\u0430\u0442\u0438\u0441\u044F \u0431\u0456\u043B\u044C\u0448\u0435 \u043F\u0440\u043E \u043F\u0440\u043E\u0454\u043A\u0442", now);
        AddTranslation(translations, "albums", "albums.metalReleases", lang, "\u041C\u0435\u0442\u0430\u043B-\u0440\u0435\u043B\u0456\u0437\u0438", now);
        AddTranslation(translations, "albums", "albums.allDistributors", lang, "\u0412\u0441\u0456", now);
        AddTranslation(translations, "albums", "albums.allDistributorsDropdown", lang, "\u0412\u0441\u0456 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438", now);
        AddTranslation(translations, "albums", "albums.filters", lang, "\u0424\u0456\u043B\u044C\u0442\u0440\u0438", now);
        AddTranslation(translations, "albums", "albums.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0430\u043B\u044C\u0431\u043E\u043C\u0438. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "albums", "albums.noAlbums", lang, "\u0410\u043B\u044C\u0431\u043E\u043C\u0456\u0432 \u0437\u0430 \u0432\u0430\u0448\u0438\u043C\u0438 \u043A\u0440\u0438\u0442\u0435\u0440\u0456\u044F\u043C\u0438 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "albums", "albums.tryAdjusting", lang, "\u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u0437\u043C\u0456\u043D\u0438\u0442\u0438 \u0444\u0456\u043B\u044C\u0442\u0440\u0438, \u0449\u043E\u0431 \u043F\u043E\u0431\u0430\u0447\u0438\u0442\u0438 \u0431\u0456\u043B\u044C\u0448\u0435 \u0440\u0435\u0437\u0443\u043B\u044C\u0442\u0430\u0442\u0456\u0432.", now);
        AddTranslation(translations, "albums", "albums.comparePrices", lang, "\u0413\u0440\u0443\u043F\u0443\u0432\u0430\u0442\u0438 \u0437\u0430 \u0430\u043B\u044C\u0431\u043E\u043C\u043E\u043C", now);
        AddTranslation(translations, "albums", "albums.searchPlaceholder", lang, "\u041F\u043E\u0448\u0443\u043A \u0437\u0430 \u043D\u0430\u0437\u0432\u043E\u044E \u0433\u0443\u0440\u0442\u0443 \u0430\u0431\u043E \u0430\u043B\u044C\u0431\u043E\u043C\u0443...", now);
        AddTranslation(translations, "albums", "albums.suggestionBand", lang, "\u0413\u0443\u0440\u0442", now);
        AddTranslation(translations, "albums", "albums.suggestionAlbum", lang, "\u0410\u043B\u044C\u0431\u043E\u043C", now);

        // grouped
        AddTranslation(translations, "grouped", "grouped.stores", lang, "\u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0456\u0432", now);
        AddTranslation(translations, "grouped", "grouped.moreStores", lang, "\u0449\u0435", now);
        AddTranslation(translations, "grouped", "grouped.showLess", lang, "\u0417\u0433\u043E\u0440\u043D\u0443\u0442\u0438", now);

        // albumCard
        AddTranslation(translations, "albumCard", "albumCard.statusNew", lang, "\u041D\u043E\u0432\u0438\u0439", now);
        AddTranslation(translations, "albumCard", "albumCard.statusRestock", lang, "\u0420\u0435\u0441\u0442\u0456\u043A", now);
        AddTranslation(translations, "albumCard", "albumCard.statusPreOrder", lang, "\u041F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaCD", lang, "CD", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaVinyl", lang, "\u0412\u0456\u043D\u0456\u043B", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaCassette", lang, "\u041A\u0430\u0441\u0435\u0442\u0430", now);
        AddTranslation(translations, "albumCard", "albumCard.mediaUnknown", lang, "\u041D\u0435\u0432\u0456\u0434\u043E\u043C\u043E", now);
        AddTranslation(translations, "albumCard", "albumCard.viewInStore", lang, "\u041A\u0443\u043F\u0438\u0442\u0438", now);

        // albumFilter
        AddTranslation(translations, "albumFilter", "albumFilter.filterAlbums", lang, "\u0424\u0456\u043B\u044C\u0442\u0440 \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432", now);
        AddTranslation(translations, "albumFilter", "albumFilter.resetFilters", lang, "\u0421\u043A\u0438\u043D\u0443\u0442\u0438 \u0444\u0456\u043B\u044C\u0442\u0440\u0438", now);
        AddTranslation(translations, "albumFilter", "albumFilter.albumName", lang, "\u041D\u0430\u0437\u0432\u0430 \u0430\u043B\u044C\u0431\u043E\u043C\u0443", now);
        AddTranslation(translations, "albumFilter", "albumFilter.searchPlaceholder", lang, "\u041F\u043E\u0448\u0443\u043A \u0437\u0430 \u043D\u0430\u0437\u0432\u043E\u044E \u0430\u043B\u044C\u0431\u043E\u043C\u0443...", now);
        AddTranslation(translations, "albumFilter", "albumFilter.from", lang, "\u0412\u0456\u0434", now);
        AddTranslation(translations, "albumFilter", "albumFilter.to", lang, "\u0414\u043E", now);
        AddTranslation(translations, "albumFilter", "albumFilter.mediaType", lang, "\u0422\u0438\u043F \u043D\u043E\u0441\u0456\u044F", now);
        AddTranslation(translations, "albumFilter", "albumFilter.all", lang, "\u0412\u0441\u0456", now);
        AddTranslation(translations, "albumFilter", "albumFilter.cd", lang, "CD", now);
        AddTranslation(translations, "albumFilter", "albumFilter.vinyl", lang, "\u0412\u0456\u043D\u0456\u043B", now);
        AddTranslation(translations, "albumFilter", "albumFilter.cassette", lang, "\u041A\u0430\u0441\u0435\u0442\u0430", now);
        AddTranslation(translations, "albumFilter", "albumFilter.status", lang, "\u0421\u0442\u0430\u0442\u0443\u0441", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusNew", lang, "\u041D\u043E\u0432\u0438\u0439", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusRestock", lang, "\u0420\u0435\u0441\u0442\u0456\u043A", now);
        AddTranslation(translations, "albumFilter", "albumFilter.statusPreorder", lang, "\u041F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "albumFilter", "albumFilter.band", lang, "\u0413\u0443\u0440\u0442", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allBands", lang, "\u0412\u0441\u0456 \u0433\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "albumFilter", "albumFilter.distributor", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allDistributors", lang, "\u0412\u0441\u0456 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438", now);
        AddTranslation(translations, "albumFilter", "albumFilter.genre", lang, "\u0416\u0430\u043D\u0440", now);
        AddTranslation(translations, "albumFilter", "albumFilter.allGenres", lang, "\u0412\u0441\u0456 \u0436\u0430\u043D\u0440\u0438", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortBy", lang, "\u0421\u043E\u0440\u0442\u0443\u0432\u0430\u0442\u0438 \u0437\u0430", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortDate", lang, "\u0414\u0430\u0442\u0430", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortName", lang, "\u041D\u0430\u0437\u0432\u0430", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortPrice", lang, "\u0426\u0456\u043D\u0430", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortBand", lang, "\u0413\u0443\u0440\u0442", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortYear", lang, "\u0420\u0456\u043A", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortDistributor", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440", now);
        AddTranslation(translations, "albumFilter", "albumFilter.sortStores", lang, "\u041C\u0430\u0433\u0430\u0437\u0438\u043D\u0438", now);
        AddTranslation(translations, "albumFilter", "albumFilter.desc", lang, "\u0421\u043F\u0430\u0434", now);
        AddTranslation(translations, "albumFilter", "albumFilter.asc", lang, "\u0417\u0440\u043E\u0441\u0442", now);
        AddTranslation(translations, "albumFilter", "albumFilter.yearRange", lang, "\u0414\u0456\u0430\u043F\u0430\u0437\u043E\u043D \u0440\u043E\u043A\u0456\u0432", now);
        AddTranslation(translations, "albumFilter", "albumFilter.minYear", lang, "\u0412\u0456\u0434", now);
        AddTranslation(translations, "albumFilter", "albumFilter.maxYear", lang, "\u0414\u043E", now);
        AddTranslation(translations, "albumFilter", "albumFilter.priceRange", lang, "\u0414\u0456\u0430\u043F\u0430\u0437\u043E\u043D \u0446\u0456\u043D", now);
        AddTranslation(translations, "albumFilter", "albumFilter.applyFilters", lang, "\u0417\u0430\u0441\u0442\u043E\u0441\u0443\u0432\u0430\u0442\u0438 \u0444\u0456\u043B\u044C\u0442\u0440\u0438", now);

        // pagination
        AddTranslation(translations, "pagination", "pagination.showing", lang, "\u041F\u043E\u043A\u0430\u0437\u0430\u043D\u043E {start}-{end} \u0437 {total} \u0435\u043B\u0435\u043C\u0435\u043D\u0442\u0456\u0432", now);
        AddTranslation(translations, "pagination", "pagination.itemsPerPage", lang, "\u0415\u043B\u0435\u043C\u0435\u043D\u0442\u0456\u0432 \u043D\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u0446\u0456:", now);
        AddTranslation(translations, "pagination", "pagination.goToFirstPage", lang, "\u041F\u0435\u0440\u0435\u0439\u0442\u0438 \u043D\u0430 \u043F\u0435\u0440\u0448\u0443 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443", now);
        AddTranslation(translations, "pagination", "pagination.goToLastPage", lang, "\u041F\u0435\u0440\u0435\u0439\u0442\u0438 \u043D\u0430 \u043E\u0441\u0442\u0430\u043D\u043D\u044E \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443", now);
        AddTranslation(translations, "pagination", "pagination.goToNextPage", lang, "\u041F\u0435\u0440\u0435\u0439\u0442\u0438 \u043D\u0430 \u043D\u0430\u0441\u0442\u0443\u043F\u043D\u0443 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443", now);
        AddTranslation(translations, "pagination", "pagination.goToPreviousPage", lang, "\u041F\u0435\u0440\u0435\u0439\u0442\u0438 \u043D\u0430 \u043F\u043E\u043F\u0435\u0440\u0435\u0434\u043D\u044E \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443", now);
        AddTranslation(translations, "pagination", "pagination.goToPage", lang, "\u041F\u0435\u0440\u0435\u0439\u0442\u0438 \u043D\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443", now);

        // distributors
        AddTranslation(translations, "distributors", "distributors.title", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438 \u043C\u0435\u0442\u0430\u043B\u0443", now);
        AddTranslation(translations, "distributors", "distributors.subtitle", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u043A\u043E\u043B\u0435\u043A\u0446\u0456\u044E \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0456\u0432 \u043C\u0435\u0442\u0430\u043B\u0443", now);
        AddTranslation(translations, "distributors", "distributors.browseProducts", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u0442\u043E\u0432\u0430\u0440\u0438", now);
        AddTranslation(translations, "distributors", "distributors.website", lang, "\u0421\u0430\u0439\u0442", now);
        AddTranslation(translations, "distributors", "distributors.products", lang, "\u0422\u043E\u0432\u0430\u0440\u0456\u0432", now);
        AddTranslation(translations, "distributors", "distributors.noDistributors", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "distributors", "distributors.checkBack", lang, "\u041F\u0435\u0440\u0435\u0432\u0456\u0440\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "distributors", "distributors.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "distributors", "distributors.fallbackDescription", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440 \u0442\u0430 \u043C\u0430\u0433\u0430\u0437\u0438\u043D \u043C\u0435\u0442\u0430\u043B\u0443.", now);

        // bands
        AddTranslation(translations, "bands", "bands.title", lang, "\u0423\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0456 \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "bands", "bands.subtitle", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u0433\u0443\u0440\u0442\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0457 \u043C\u0435\u0442\u0430\u043B-\u0441\u0446\u0435\u043D\u0438", now);
        AddTranslation(translations, "bands", "bands.album", lang, "\u0430\u043B\u044C\u0431\u043E\u043C", now);
        AddTranslation(translations, "bands", "bands.albums", lang, "\u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432", now);
        AddTranslation(translations, "bands", "bands.browseAlbums", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u0430\u043B\u044C\u0431\u043E\u043C\u0438", now);
        AddTranslation(translations, "bands", "bands.noBands", lang, "\u0413\u0443\u0440\u0442\u0456\u0432 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "bands", "bands.checkBack", lang, "\u041F\u0435\u0440\u0435\u0432\u0456\u0440\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "bands", "bands.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0433\u0443\u0440\u0442\u0438. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "bands", "bands.noDescription", lang, "\u041E\u043F\u0438\u0441 \u0432\u0456\u0434\u0441\u0443\u0442\u043D\u0456\u0439.", now);
        AddTranslation(translations, "bands", "bands.viewAlbums", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u0430\u043B\u044C\u0431\u043E\u043C\u0438", now);
        AddTranslation(translations, "bands", "bands.heading", lang, "\u041C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "bands", "bands.headingSubtitle", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u043A\u043E\u043B\u0435\u043A\u0446\u0456\u044E \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0456\u0432 \u0442\u0430 \u0432\u0456\u0434\u043A\u0440\u0438\u0432\u0430\u0439\u0442\u0435 \u0457\u0445\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438. \u041A\u0456\u043B\u044C\u043A\u0456\u0441\u0442\u044C \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432 \u0432\u0456\u0434\u043E\u0431\u0440\u0430\u0436\u0430\u0454 \u043B\u0438\u0448\u0435 \u0440\u0435\u043B\u0456\u0437\u0438, \u0434\u043E\u0441\u0442\u0443\u043F\u043D\u0456 \u0443 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432, \u044F\u043A\u0456 \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0443\u0454 \u0446\u0435\u0439 \u0445\u0430\u0431, \u0430 \u043D\u0435 \u043F\u043E\u0432\u043D\u0443 \u0434\u0438\u0441\u043A\u043E\u0433\u0440\u0430\u0444\u0456\u044E \u0433\u0443\u0440\u0442\u0443.", now);
        AddTranslation(translations, "bands", "bands.searchPlaceholder", lang, "\u041F\u043E\u0448\u0443\u043A \u0437\u0430 \u043D\u0430\u0437\u0432\u043E\u044E \u0433\u0443\u0440\u0442\u0443...", now);
        AddTranslation(translations, "bands", "bands.noResults", lang, "\u0413\u0443\u0440\u0442\u0456\u0432 \u0437\u0430 \u0432\u0430\u0448\u0438\u043C \u0437\u0430\u043F\u0438\u0442\u043E\u043C \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);

        // collection
        AddTranslation(translations, "collection", "collection.favorite", lang, "\u0423\u043B\u044E\u0431\u043B\u0435\u043D\u0435", now);
        AddTranslation(translations, "collection", "collection.want", lang, "\u0425\u043E\u0447\u0443", now);
        AddTranslation(translations, "collection", "collection.owned", lang, "\u041C\u0430\u044E", now);
        AddTranslation(translations, "collection", "collection.remove", lang, "\u0412\u0438\u0434\u0430\u043B\u0438\u0442\u0438", now);

        // profile
        AddTranslation(translations, "profile", "profile.loading", lang, "\u0417\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0435\u043D\u043D\u044F \u043F\u0440\u043E\u0444\u0456\u043B\u044E...", now);
        AddTranslation(translations, "profile", "profile.authInfo", lang, "\u0406\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0456\u044F \u043F\u0440\u043E \u0430\u0432\u0442\u0435\u043D\u0442\u0438\u0444\u0456\u043A\u0430\u0446\u0456\u044E", now);
        AddTranslation(translations, "profile", "profile.loginTime", lang, "\u0427\u0430\u0441 \u0432\u0445\u043E\u0434\u0443", now);
        AddTranslation(translations, "profile", "profile.sessionValidUntil", lang, "\u0421\u0435\u0441\u0456\u044F \u0434\u0456\u0439\u0441\u043D\u0430 \u0434\u043E", now);
        AddTranslation(translations, "profile", "profile.signOut", lang, "\u0412\u0438\u0439\u0442\u0438", now);
        AddTranslation(translations, "profile", "profile.userInfo", lang, "\u0406\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0456\u044F \u043F\u0440\u043E \u043A\u043E\u0440\u0438\u0441\u0442\u0443\u0432\u0430\u0447\u0430", now);
        AddTranslation(translations, "profile", "profile.userId", lang, "ID \u043A\u043E\u0440\u0438\u0441\u0442\u0443\u0432\u0430\u0447\u0430", now);
        AddTranslation(translations, "profile", "profile.email", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430", now);
        AddTranslation(translations, "profile", "profile.username", lang, "\u0406\u043C'\u044F \u043A\u043E\u0440\u0438\u0441\u0442\u0443\u0432\u0430\u0447\u0430", now);
        AddTranslation(translations, "profile", "profile.emailNotProvided", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430 \u043D\u0435 \u0432\u043A\u0430\u0437\u0430\u043D\u0430", now);
        AddTranslation(translations, "profile", "profile.favorites", lang, "\u0423\u043B\u044E\u0431\u043B\u0435\u043D\u0456", now);
        AddTranslation(translations, "profile", "profile.wishlist", lang, "\u0421\u043F\u0438\u0441\u043E\u043A \u0431\u0430\u0436\u0430\u043D\u044C", now);
        AddTranslation(translations, "profile", "profile.collection", lang, "\u041A\u043E\u043B\u0435\u043A\u0446\u0456\u044F", now);
        AddTranslation(translations, "profile", "profile.profileTab", lang, "\u041F\u0440\u043E\u0444\u0456\u043B\u044C", now);
        AddTranslation(translations, "profile", "profile.exportCsv", lang, "\u0415\u043A\u0441\u043F\u043E\u0440\u0442 CSV", now);

        // reviews
        AddTranslation(translations, "reviews", "reviews.title", lang, "\u0412\u0456\u0434\u0433\u0443\u043A\u0438", now);
        AddTranslation(translations, "reviews", "reviews.subtitle", lang, "\u041F\u043E\u0434\u0456\u043B\u0456\u0442\u044C\u0441\u044F \u0432\u0440\u0430\u0436\u0435\u043D\u043D\u044F\u043C\u0438 \u043F\u0440\u043E Metal Release Tracker", now);
        AddTranslation(translations, "reviews", "reviews.messageLabel", lang, "\u0412\u0430\u0448 \u0432\u0456\u0434\u0433\u0443\u043A", now);
        AddTranslation(translations, "reviews", "reviews.messagePlaceholder", lang, "\u041F\u043E\u0434\u0456\u043B\u0456\u0442\u044C\u0441\u044F \u0434\u0443\u043C\u043A\u0430\u043C\u0438 \u043F\u0440\u043E \u043F\u0440\u043E\u0454\u043A\u0442...", now);
        AddTranslation(translations, "reviews", "reviews.submit", lang, "\u041D\u0430\u0434\u0456\u0441\u043B\u0430\u0442\u0438", now);
        AddTranslation(translations, "reviews", "reviews.sending", lang, "\u041D\u0430\u0434\u0441\u0438\u043B\u0430\u043D\u043D\u044F...", now);
        AddTranslation(translations, "reviews", "reviews.success", lang, "\u0414\u044F\u043A\u0443\u0454\u043C\u043E! \u0412\u0430\u0448 \u0432\u0456\u0434\u0433\u0443\u043A \u043E\u043F\u0443\u0431\u043B\u0456\u043A\u043E\u0432\u0430\u043D\u043E.", now);
        AddTranslation(translations, "reviews", "reviews.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u043D\u0430\u0434\u0456\u0441\u043B\u0430\u0442\u0438 \u0432\u0456\u0434\u0433\u0443\u043A. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "reviews", "reviews.loginRequired", lang, "\u0423\u0432\u0456\u0439\u0434\u0456\u0442\u044C, \u0449\u043E\u0431 \u0437\u0430\u043B\u0438\u0448\u0438\u0442\u0438 \u0432\u0456\u0434\u0433\u0443\u043A.", now);
        AddTranslation(translations, "reviews", "reviews.listTitle", lang, "\u0412\u0456\u0434\u0433\u0443\u043A\u0438 \u043A\u043E\u0440\u0438\u0441\u0442\u0443\u0432\u0430\u0447\u0456\u0432", now);
        AddTranslation(translations, "reviews", "reviews.empty", lang, "\u0412\u0456\u0434\u0433\u0443\u043A\u0456\u0432 \u0449\u0435 \u043D\u0435\u043C\u0430\u0454. \u0411\u0443\u0434\u044C\u0442\u0435 \u043F\u0435\u0440\u0448\u0438\u043C\u0438!", now);

        // changelog
        AddTranslation(translations, "changelog", "changelog.title", lang, "\u0416\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D", now);
        AddTranslation(translations, "changelog", "changelog.subtitle", lang, "\u0425\u0440\u043E\u043D\u043E\u043B\u043E\u0433\u0456\u044F \u043F\u043E\u0434\u0456\u0439 \u0441\u0438\u043D\u0445\u0440\u043E\u043D\u0456\u0437\u0430\u0446\u0456\u0457 \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432 \u0437 \u0443\u0441\u0456\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432.", now);
        AddTranslation(translations, "changelog", "changelog.date", lang, "\u0414\u0430\u0442\u0430", now);
        AddTranslation(translations, "changelog", "changelog.band", lang, "\u0413\u0443\u0440\u0442", now);
        AddTranslation(translations, "changelog", "changelog.album", lang, "\u0410\u043B\u044C\u0431\u043E\u043C", now);
        AddTranslation(translations, "changelog", "changelog.price", lang, "\u0426\u0456\u043D\u0430", now);
        AddTranslation(translations, "changelog", "changelog.distributor", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440", now);
        AddTranslation(translations, "changelog", "changelog.status", lang, "\u0421\u0442\u0430\u0442\u0443\u0441", now);
        AddTranslation(translations, "changelog", "changelog.statusNew", lang, "\u041D\u043E\u0432\u0438\u0439", now);
        AddTranslation(translations, "changelog", "changelog.statusUpdated", lang, "\u041E\u043D\u043E\u0432\u043B\u0435\u043D\u043E", now);
        AddTranslation(translations, "changelog", "changelog.statusDeleted", lang, "\u0412\u0438\u0434\u0430\u043B\u0435\u043D\u043E", now);
        AddTranslation(translations, "changelog", "changelog.empty", lang, "\u0417\u0430\u043F\u0438\u0441\u0456\u0432 \u0443 \u0436\u0443\u0440\u043D\u0430\u043B\u0456 \u0437\u043C\u0456\u043D \u0449\u0435 \u043D\u0435\u043C\u0430\u0454", now);
        AddTranslation(translations, "changelog", "changelog.emptyHint", lang, "\u0417\u0430\u043F\u0438\u0441\u0438 \u0437'\u044F\u0432\u043B\u044F\u0442\u044C\u0441\u044F, \u043A\u043E\u043B\u0438 \u0430\u043B\u044C\u0431\u043E\u043C\u0438 \u0431\u0443\u0434\u0443\u0442\u044C \u0441\u0438\u043D\u0445\u0440\u043E\u043D\u0456\u0437\u043E\u0432\u0430\u043D\u0456 \u0437 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430\u043C\u0438.", now);
        AddTranslation(translations, "changelog", "changelog.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0436\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);

        // favorites
        AddTranslation(translations, "favorites", "favorites.empty", lang, "\u041E\u0431\u0440\u0430\u043D\u0438\u0445 \u0449\u0435 \u043D\u0435\u043C\u0430\u0454", now);
        AddTranslation(translations, "favorites", "favorites.emptyHint", lang, "\u041D\u0430\u0442\u0438\u0441\u043D\u0456\u0442\u044C \u043D\u0430 \u0441\u0435\u0440\u0446\u0435 \u043D\u0430 \u043A\u0430\u0440\u0442\u043A\u0430\u0445 \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432, \u0449\u043E\u0431 \u0434\u043E\u0434\u0430\u0442\u0438 \u0457\u0445 \u0434\u043E \u043E\u0431\u0440\u0430\u043D\u043E\u0433\u043E.", now);

        // login
        AddTranslation(translations, "login", "login.title", lang, "\u0423\u0432\u0456\u0439\u0442\u0438", now);
        AddTranslation(translations, "login", "login.or", lang, "\u0410\u0411\u041E", now);
        AddTranslation(translations, "login", "login.emailLabel", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430", now);
        AddTranslation(translations, "login", "login.passwordLabel", lang, "\u041F\u0430\u0440\u043E\u043B\u044C", now);
        AddTranslation(translations, "login", "login.rememberMe", lang, "\u0417\u0430\u043F\u0430\u043C'\u044F\u0442\u0430\u0442\u0438 \u043C\u0435\u043D\u0435", now);
        AddTranslation(translations, "login", "login.signIn", lang, "\u0423\u0432\u0456\u0439\u0442\u0438", now);
        AddTranslation(translations, "login", "login.noAccount", lang, "\u041D\u0435\u043C\u0430\u0454 \u0430\u043A\u0430\u0443\u043D\u0442\u0430?", now);
        AddTranslation(translations, "login", "login.signUp", lang, "\u0417\u0430\u0440\u0435\u0454\u0441\u0442\u0440\u0443\u0432\u0430\u0442\u0438\u0441\u044F", now);
        AddTranslation(translations, "login", "login.validation", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430 \u0442\u0430 \u043F\u0430\u0440\u043E\u043B\u044C \u043E\u0431\u043E\u0432'\u044F\u0437\u043A\u043E\u0432\u0456", now);

        // register
        AddTranslation(translations, "register", "register.title", lang, "\u0420\u0435\u0454\u0441\u0442\u0440\u0430\u0446\u0456\u044F", now);
        AddTranslation(translations, "register", "register.or", lang, "\u0410\u0411\u041E", now);
        AddTranslation(translations, "register", "register.emailLabel", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430", now);
        AddTranslation(translations, "register", "register.displayName", lang, "\u0406\u043C'\u044F \u0434\u043B\u044F \u0432\u0456\u0434\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u043D\u044F (\u043D\u0435\u043E\u0431\u043E\u0432'\u044F\u0437\u043A\u043E\u0432\u043E)", now);
        AddTranslation(translations, "register", "register.displayNameHelper", lang, "\u042F\u043A\u0449\u043E \u0437\u0430\u043B\u0438\u0448\u0438\u0442\u0438 \u043F\u043E\u0440\u043E\u0436\u043D\u0456\u043C, \u0431\u0443\u0434\u0435 \u0432\u0438\u043A\u043E\u0440\u0438\u0441\u0442\u0430\u043D\u043E \u0432\u0430\u0448\u0443 \u043F\u043E\u0448\u0442\u0443", now);
        AddTranslation(translations, "register", "register.passwordLabel", lang, "\u041F\u0430\u0440\u043E\u043B\u044C", now);
        AddTranslation(translations, "register", "register.confirmPassword", lang, "\u041F\u0456\u0434\u0442\u0432\u0435\u0440\u0434\u0436\u0435\u043D\u043D\u044F \u043F\u0430\u0440\u043E\u043B\u044F", now);
        AddTranslation(translations, "register", "register.submit", lang, "\u0417\u0430\u0440\u0435\u0454\u0441\u0442\u0440\u0443\u0432\u0430\u0442\u0438\u0441\u044F", now);
        AddTranslation(translations, "register", "register.hasAccount", lang, "\u0412\u0436\u0435 \u043C\u0430\u0454\u0442\u0435 \u0430\u043A\u0430\u0443\u043D\u0442?", now);
        AddTranslation(translations, "register", "register.signIn", lang, "\u0423\u0432\u0456\u0439\u0442\u0438", now);
        AddTranslation(translations, "register", "register.validationRequired", lang, "\u0415\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430 \u043F\u043E\u0448\u0442\u0430, \u043F\u0430\u0440\u043E\u043B\u044C \u0442\u0430 \u043F\u0456\u0434\u0442\u0432\u0435\u0440\u0434\u0436\u0435\u043D\u043D\u044F \u043F\u0430\u0440\u043E\u043B\u044F \u043E\u0431\u043E\u0432'\u044F\u0437\u043A\u043E\u0432\u0456", now);
        AddTranslation(translations, "register", "register.validationMismatch", lang, "\u041F\u0430\u0440\u043E\u043B\u0456 \u043D\u0435 \u0437\u0431\u0456\u0433\u0430\u044E\u0442\u044C\u0441\u044F", now);

        // about
        AddTranslation(translations, "about", "about.title", lang, "Metal Release Tracker", now);
        AddTranslation(translations, "about", "about.heroSubtitle", lang, "\u0426\u0435\u043D\u0442\u0440\u0430\u043B\u0456\u0437\u043E\u0432\u0430\u043D\u0438\u0439 \u0445\u0430\u0431 \u0434\u043B\u044F \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0435\u043D\u043D\u044F \u0440\u0435\u043B\u0456\u0437\u0456\u0432 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443, \u0449\u043E \u043F\u0440\u043E\u0434\u0430\u044E\u0442\u044C\u0441\u044F \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u043C\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430\u043C\u0438 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0430\u043C\u0438.", now);
        AddTranslation(translations, "about", "about.problemTitle", lang, "\u041F\u0440\u043E\u0431\u043B\u0435\u043C\u0430", now);
        AddTranslation(translations, "about", "about.problemText", lang, "\u0423\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0456 \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0438 \u0432\u0438\u043F\u0443\u0441\u043A\u0430\u044E\u0442\u044C \u043D\u0435\u0439\u043C\u043E\u0432\u0456\u0440\u043D\u0443 \u043C\u0443\u0437\u0438\u043A\u0443, \u0430\u043B\u0435 \u0457\u0445\u043D\u0456 \u0444\u0456\u0437\u0438\u0447\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 (\u0432\u0456\u043D\u0456\u043B, CD, \u043A\u0430\u0441\u0435\u0442\u0438) \u0447\u0430\u0441\u0442\u043E \u0440\u043E\u0437\u043F\u043E\u0432\u0441\u044E\u0434\u0436\u0443\u044E\u0442\u044C\u0441\u044F \u0432\u0438\u043A\u043B\u044E\u0447\u043D\u043E \u0447\u0435\u0440\u0435\u0437 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0456 \u043B\u0435\u0439\u0431\u043B\u0438 \u0442\u0430 \u0434\u0438\u0441\u0442\u0440\u043E \u043F\u043E \u0432\u0441\u0456\u0439 \u0404\u0432\u0440\u043E\u043F\u0456. \u0414\u043B\u044F \u0444\u0430\u043D\u0430\u0442\u0456\u0432 \u0432 \u0423\u043A\u0440\u0430\u0457\u043D\u0456 \u0442\u0430 \u043F\u043E \u0432\u0441\u044C\u043E\u043C\u0443 \u0441\u0432\u0456\u0442\u0443 \u043F\u043E\u0448\u0443\u043A \u0434\u0435 \u043A\u0443\u043F\u0438\u0442\u0438 \u0446\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u043E\u0437\u043D\u0430\u0447\u0430\u0454 \u0440\u0443\u0447\u043D\u0443 \u043F\u0435\u0440\u0435\u0432\u0456\u0440\u043A\u0443 \u0434\u0435\u0441\u044F\u0442\u043A\u0456\u0432 \u043E\u043D\u043B\u0430\u0439\u043D-\u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0456\u0432. \u0420\u0435\u043B\u0456\u0437\u0438 \u0437'\u044F\u0432\u043B\u044F\u044E\u0442\u044C\u0441\u044F \u0456 \u0437\u043D\u0438\u043A\u0430\u044E\u0442\u044C, \u0456 \u0447\u0430\u0441\u0442\u043E \u0432\u0438 \u0434\u0456\u0437\u043D\u0430\u0454\u0442\u0435\u0441\u044C \u043F\u0440\u043E \u043D\u0438\u0445, \u043A\u043E\u043B\u0438 \u0432\u0436\u0435 \u0432\u0441\u0435 \u0440\u043E\u0437\u043F\u0440\u043E\u0434\u0430\u043D\u043E.", now);
        AddTranslation(translations, "about", "about.solutionTitle", lang, "\u0420\u0456\u0448\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "about", "about.solutionText", lang, "Metal Release Tracker \u0430\u0432\u0442\u043E\u043C\u0430\u0442\u0438\u0447\u043D\u043E \u0441\u043A\u0430\u043D\u0443\u0454 \u043A\u0430\u0442\u0430\u043B\u043E\u0433\u0438 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432, \u0437\u043D\u0430\u0445\u043E\u0434\u0438\u0442\u044C \u043A\u043E\u0436\u0435\u043D \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u0440\u0435\u043B\u0456\u0437 \u0456 \u043F\u0440\u0435\u0434\u0441\u0442\u0430\u0432\u043B\u044F\u0454 \u0457\u0445 \u0443 \u0454\u0434\u0438\u043D\u043E\u043C\u0443 \u043A\u0430\u0442\u0430\u043B\u043E\u0437\u0456 \u0437 \u043F\u043E\u0448\u0443\u043A\u043E\u043C. \u041A\u043E\u0436\u0435\u043D \u0440\u0435\u043B\u0456\u0437 \u043C\u0430\u0454 \u043F\u0440\u044F\u043C\u0435 \u043F\u043E\u0441\u0438\u043B\u0430\u043D\u043D\u044F \u043D\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u0442\u043E\u0432\u0430\u0440\u0443 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430. \u041D\u043E\u0432\u0456 \u0440\u0435\u043B\u0456\u0437\u0438, \u0440\u0435\u0441\u0442\u043E\u043A\u0438 \u0442\u0430 \u043F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0443\u044E\u0442\u044C\u0441\u044F \u0431\u0435\u0437\u043F\u0435\u0440\u0435\u0440\u0432\u043D\u043E.", now);
        AddTranslation(translations, "about", "about.howItWorks", lang, "\u042F\u043A \u0446\u0435 \u043F\u0440\u0430\u0446\u044E\u0454", now);
        AddTranslation(translations, "about", "about.features.discover.title", lang, "\u0417\u043D\u0430\u0445\u043E\u0434\u044C\u0442\u0435", now);
        AddTranslation(translations, "about", "about.features.discover.description", lang, "\u0417\u043D\u0430\u0445\u043E\u0434\u044C\u0442\u0435 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432, \u044F\u043A\u0456 \u0456\u043D\u0430\u043A\u0448\u0435 \u0432\u0430\u0436\u043A\u043E \u0432\u0456\u0434\u0441\u0442\u0435\u0436\u0438\u0442\u0438.", now);
        AddTranslation(translations, "about", "about.features.globalReach.title", lang, "\u0413\u043B\u043E\u0431\u0430\u043B\u044C\u043D\u0435 \u043E\u0445\u043E\u043F\u043B\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "about", "about.features.globalReach.description", lang, "\u041C\u0438 \u0430\u0433\u0440\u0435\u0433\u0443\u0454\u043C\u043E \u043A\u0430\u0442\u0430\u043B\u043E\u0433\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432 \u0437 \u0443\u0441\u0456\u0454\u0457 \u0404\u0432\u0440\u043E\u043F\u0438 \u0442\u0430 \u043D\u0435 \u0442\u0456\u043B\u044C\u043A\u0438.", now);
        AddTranslation(translations, "about", "about.features.orderDirect.title", lang, "\u0417\u0430\u043C\u043E\u0432\u043B\u044F\u0439\u0442\u0435 \u043D\u0430\u043F\u0440\u044F\u043C\u0443", now);
        AddTranslation(translations, "about", "about.features.orderDirect.description", lang, "\u041F\u0435\u0440\u0435\u0445\u043E\u0434\u044C\u0442\u0435 \u043F\u0440\u044F\u043C\u043E \u043D\u0430 \u0441\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0443 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430 \u0442\u0430 \u0437\u0430\u043C\u043E\u0432\u043B\u044F\u0439\u0442\u0435 \u0444\u0456\u0437\u0438\u0447\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u0431\u0435\u0437 \u043F\u043E\u0441\u0435\u0440\u0435\u0434\u043D\u0438\u043A\u0456\u0432.", now);
        AddTranslation(translations, "about", "about.features.stayUpdated.title", lang, "\u0411\u0443\u0434\u044C\u0442\u0435 \u0432 \u043A\u0443\u0440\u0441\u0456", now);
        AddTranslation(translations, "about", "about.features.stayUpdated.description", lang, "\u041D\u0430\u0448\u0456 \u0430\u0432\u0442\u043E\u043C\u0430\u0442\u0438\u0447\u043D\u0456 \u043F\u0430\u0440\u0441\u0435\u0440\u0438 \u0431\u0435\u0437\u043F\u0435\u0440\u0435\u0440\u0432\u043D\u043E \u0441\u043A\u0430\u043D\u0443\u044E\u0442\u044C \u043A\u0430\u0442\u0430\u043B\u043E\u0433\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432, \u0449\u043E\u0431 \u0432\u0438 \u043D\u0435 \u043F\u0440\u043E\u043F\u0443\u0441\u0442\u0438\u043B\u0438 \u043D\u043E\u0432\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u0442\u0430 \u0440\u0435\u0441\u0442\u043E\u043A\u0438.", now);
        AddTranslation(translations, "about", "about.features.allFormats.title", lang, "\u0412\u0441\u0456 \u0444\u043E\u0440\u043C\u0430\u0442\u0438", now);
        AddTranslation(translations, "about", "about.features.allFormats.description", lang, "\u0412\u0456\u043D\u0456\u043B, CD, \u043A\u0430\u0441\u0435\u0442\u0438 - \u043F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443 \u0432\u0441\u0456\u0445 \u0444\u043E\u0440\u043C\u0430\u0442\u0430\u0445 \u0444\u0456\u0437\u0438\u0447\u043D\u0438\u0445 \u043D\u043E\u0441\u0456\u0457\u0432 \u0432 \u043E\u0434\u043D\u043E\u043C\u0443 \u043A\u0430\u0442\u0430\u043B\u043E\u0437\u0456.", now);
        AddTranslation(translations, "about", "about.features.forCommunity.title", lang, "\u0414\u043B\u044F \u0441\u043F\u0456\u043B\u044C\u043D\u043E\u0442\u0438", now);
        AddTranslation(translations, "about", "about.features.forCommunity.description", lang, "\u0421\u0442\u0432\u043E\u0440\u0435\u043D\u043E \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u043C\u0438 \u043C\u0435\u0442\u0430\u043B\u0445\u0435\u0434\u0430\u043C\u0438 \u0434\u043B\u044F \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u0445 \u043C\u0435\u0442\u0430\u043B\u0445\u0435\u0434\u0456\u0432. \u041F\u0456\u0434\u0442\u0440\u0438\u043C\u0443\u0454\u043C\u043E \u0441\u0446\u0435\u043D\u0443, \u0440\u043E\u0431\u043B\u044F\u0447\u0438 \u0457\u0457 \u043C\u0443\u0437\u0438\u043A\u0443 \u0434\u043E\u0441\u0442\u0443\u043F\u043D\u0456\u0448\u043E\u044E.", now);
        AddTranslation(translations, "about", "about.networkTitle", lang, "\u0417\u0440\u043E\u0441\u0442\u0430\u044E\u0447\u0430 \u043C\u0435\u0440\u0435\u0436\u0430 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432", now);
        AddTranslation(translations, "about", "about.networkText", lang, "\u041C\u0438 \u043F\u043E\u0441\u0442\u0456\u0439\u043D\u043E \u0434\u043E\u0434\u0430\u0454\u043C\u043E \u043D\u043E\u0432\u0438\u0445 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432, \u044F\u043A\u0456 \u043C\u0430\u044E\u0442\u044C \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443. \u041D\u0430\u0448\u0430 \u0441\u0438\u0441\u0442\u0435\u043C\u0430 \u043C\u043E\u043D\u0456\u0442\u043E\u0440\u0438\u0442\u044C \u0457\u0445\u043D\u0456 \u043A\u0430\u0442\u0430\u043B\u043E\u0433\u0438 \u0446\u0456\u043B\u043E\u0434\u043E\u0431\u043E\u0432\u043E.", now);
        AddTranslation(translations, "about", "about.supportTitle", lang, "\u041F\u0456\u0434\u0442\u0440\u0438\u043C\u0430\u0439\u0442\u0435 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B", now);
        AddTranslation(translations, "about", "about.supportText", lang, "\u041A\u043E\u0436\u043D\u0430 \u043F\u043E\u043A\u0443\u043F\u043A\u0430 \u0443 \u043B\u0435\u0433\u0456\u0442\u0438\u043C\u043D\u043E\u0433\u043E \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430 \u043F\u0456\u0434\u0442\u0440\u0438\u043C\u0443\u0454 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u0445 \u0430\u0440\u0442\u0438\u0441\u0442\u0456\u0432 \u0442\u0430 \u0433\u043B\u043E\u0431\u0430\u043B\u044C\u043D\u0443 \u043C\u0435\u0442\u0430\u043B-\u0441\u043F\u0456\u043B\u044C\u043D\u043E\u0442\u0443. \u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u044C\u0442\u0435 \u043A\u0430\u0442\u0430\u043B\u043E\u0433, \u0437\u043D\u0430\u0439\u0434\u0456\u0442\u044C \u0449\u043E\u0441\u044C \u0432\u0430\u0436\u043A\u0435 \u0442\u0430 \u0437\u0430\u043C\u043E\u0432\u0442\u0435 \u043D\u0430\u043F\u0440\u044F\u043C\u0443.", now);

        // pageMeta
        AddTranslation(translations, "pageMeta", "pageMeta.homeTitle", lang, "\u0420\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432", now);
        AddTranslation(translations, "pageMeta", "pageMeta.homeDescription", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432. \u0424\u0456\u043B\u044C\u0442\u0440\u0443\u0439\u0442\u0435 \u0437\u0430 \u0433\u0443\u0440\u0442\u043E\u043C, \u0444\u043E\u0440\u043C\u0430\u0442\u043E\u043C, \u0446\u0456\u043D\u043E\u044E. \u0412\u0456\u043D\u0456\u043B, CD, \u043A\u0430\u0441\u0435\u0442\u0438 - \u0437\u0430\u043C\u043E\u0432\u043B\u044F\u0439\u0442\u0435 \u043D\u0430\u043F\u0440\u044F\u043C\u0443.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.albumsTitle", lang, "\u0410\u043B\u044C\u0431\u043E\u043C\u0438 - \u0420\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443", now);
        AddTranslation(translations, "pageMeta", "pageMeta.albumsDescription", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u0434\u0430\u0439\u0442\u0435 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432. \u0424\u0456\u043B\u044C\u0442\u0440\u0443\u0439\u0442\u0435 \u0437\u0430 \u0433\u0443\u0440\u0442\u043E\u043C, \u0444\u043E\u0440\u043C\u0430\u0442\u043E\u043C, \u0446\u0456\u043D\u043E\u044E. \u0412\u0456\u043D\u0456\u043B, CD, \u043A\u0430\u0441\u0435\u0442\u0438 - \u0437\u0430\u043C\u043E\u0432\u043B\u044F\u0439\u0442\u0435 \u043D\u0430\u043F\u0440\u044F\u043C\u0443.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.bandsTitle", lang, "\u0413\u0443\u0440\u0442\u0438 - \u0423\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0456 \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "pageMeta", "pageMeta.bandsDescription", lang, "\u0414\u043E\u0441\u043B\u0456\u0434\u0436\u0443\u0439\u0442\u0435 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0456 \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0438, \u0447\u0438\u0457 \u0444\u0456\u0437\u0438\u0447\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u043F\u0440\u043E\u0434\u0430\u044E\u0442\u044C\u0441\u044F \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u043C\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430\u043C\u0438 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0430\u043C\u0438.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.distributorsTitle", lang, "\u0414\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438 - \u0417\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0456 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B\u0438 \u0442\u0430 \u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0438", now);
        AddTranslation(translations, "pageMeta", "pageMeta.distributorsDescription", lang, "\u0417\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0456 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0438 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0438, \u0449\u043E \u043F\u0440\u043E\u0434\u0430\u044E\u0442\u044C \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443. Osmose Productions, Drakkar, Black Metal Vendor \u0442\u0430 \u0456\u043D\u0448\u0456.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.newsTitle", lang, "\u041D\u043E\u0432\u0438\u043D\u0438", now);
        AddTranslation(translations, "pageMeta", "pageMeta.newsDescription", lang, "\u041E\u0441\u0442\u0430\u043D\u043D\u0456 \u043D\u043E\u0432\u0438\u043D\u0438 \u0442\u0430 \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u043D\u044F Metal Release Tracker.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.aboutTitle", lang, "\u041F\u0440\u043E \u043F\u0440\u043E\u0454\u043A\u0442", now);
        AddTranslation(translations, "pageMeta", "pageMeta.aboutDescription", lang, "Metal Release Tracker \u0430\u0433\u0440\u0435\u0433\u0443\u0454 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u043E\u0433\u043E \u043C\u0435\u0442\u0430\u043B\u0443 \u0432\u0456\u0434 \u0437\u0430\u043A\u043E\u0440\u0434\u043E\u043D\u043D\u0438\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432 \u0442\u0430 \u043B\u0435\u0439\u0431\u043B\u0456\u0432 \u0432 \u0454\u0434\u0438\u043D\u0438\u0439 \u043A\u0430\u0442\u0430\u043B\u043E\u0433 \u0437 \u043F\u043E\u0448\u0443\u043A\u043E\u043C.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.reviewsTitle", lang, "\u0412\u0456\u0434\u0433\u0443\u043A\u0438", now);
        AddTranslation(translations, "pageMeta", "pageMeta.reviewsDescription", lang, "\u0427\u0438\u0442\u0430\u0439\u0442\u0435 \u0442\u0430 \u0437\u0430\u043B\u0438\u0448\u0430\u0439\u0442\u0435 \u0432\u0456\u0434\u0433\u0443\u043A\u0438 \u043F\u0440\u043E Metal Release Tracker.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.changelogTitle", lang, "\u0416\u0443\u0440\u043D\u0430\u043B \u0437\u043C\u0456\u043D", now);
        AddTranslation(translations, "pageMeta", "pageMeta.changelogDescription", lang, "\u0425\u0440\u043E\u043D\u043E\u043B\u043E\u0433\u0456\u044F \u043F\u043E\u0434\u0456\u0439 \u0441\u0438\u043D\u0445\u0440\u043E\u043D\u0456\u0437\u0430\u0446\u0456\u0457 \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432. \u0412\u0456\u0434\u0441\u0442\u0435\u0436\u0443\u0439\u0442\u0435 \u043D\u043E\u0432\u0456, \u043E\u043D\u043E\u0432\u043B\u0435\u043D\u0456 \u0442\u0430 \u0432\u0438\u0434\u0430\u043B\u0435\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u0432\u0456\u0434 \u0443\u0441\u0456\u0445 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0456\u0432.", now);
        AddTranslation(translations, "pageMeta", "pageMeta.loginTitle", lang, "\u0423\u0432\u0456\u0439\u0442\u0438", now);
        AddTranslation(translations, "pageMeta", "pageMeta.registerTitle", lang, "\u0420\u0435\u0454\u0441\u0442\u0440\u0430\u0446\u0456\u044F", now);
        AddTranslation(translations, "pageMeta", "pageMeta.notFoundTitle", lang, "\u0421\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E", now);

        // newArrivals
        AddTranslation(translations, "newArrivals", "newArrivals.title", lang, "\u041D\u043E\u0432\u0456 \u043D\u0430\u0434\u0445\u043E\u0434\u0436\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "newArrivals", "newArrivals.subtitle", lang, "\u0414\u043E\u0434\u0430\u043D\u043E \u0437\u0430 \u043E\u0441\u0442\u0430\u043D\u043D\u0456 14 \u0434\u043D\u0456\u0432", now);
        AddTranslation(translations, "newArrivals", "newArrivals.viewAll", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u0432\u0441\u0456 \u043D\u043E\u0432\u0456 \u043D\u0430\u0434\u0445\u043E\u0434\u0436\u0435\u043D\u043D\u044F", now);

        // recentlyViewed
        AddTranslation(translations, "recentlyViewed", "recentlyViewed.title", lang, "\u041D\u0435\u0449\u043E\u0434\u0430\u0432\u043D\u043E \u043F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0456", now);

        // bandDetail
        AddTranslation(translations, "bandDetail", "bandDetail.albumsBy", lang, "\u0420\u0435\u043B\u0456\u0437\u0438 \u0433\u0443\u0440\u0442\u0443 {bandName}", now);
        AddTranslation(translations, "bandDetail", "bandDetail.noAlbums", lang, "\u0420\u0435\u043B\u0456\u0437\u0456\u0432 \u0446\u044C\u043E\u0433\u043E \u0433\u0443\u0440\u0442\u0443 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.backToBands", lang, "\u041D\u0430\u0437\u0430\u0434 \u0434\u043E \u0433\u0443\u0440\u0442\u0456\u0432", now);
        AddTranslation(translations, "bandDetail", "bandDetail.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0456\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0456\u044E \u043F\u0440\u043E \u0433\u0443\u0440\u0442. \u0421\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u043F\u0456\u0437\u043D\u0456\u0448\u0435.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.notFound", lang, "\u0413\u0443\u0440\u0442 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "bandDetail", "bandDetail.similarBands", lang, "\u0421\u0445\u043E\u0436\u0456 \u0433\u0443\u0440\u0442\u0438", now);
        AddTranslation(translations, "bandDetail", "bandDetail.follow", lang, "\u0421\u0442\u0435\u0436\u0438\u0442\u0438", now);
        AddTranslation(translations, "bandDetail", "bandDetail.following", lang, "\u0421\u0442\u0435\u0436\u0443", now);
        AddTranslation(translations, "bandDetail", "bandDetail.follower", lang, "\u043F\u0456\u0434\u043F\u0438\u0441\u043D\u0438\u043A", now);
        AddTranslation(translations, "bandDetail", "bandDetail.followers", lang, "\u043F\u0456\u0434\u043F\u0438\u0441\u043D\u0438\u043A\u0456\u0432", now);
        AddTranslation(translations, "bandDetail", "bandDetail.viewOnMetalArchives", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u043D\u0430 Metal Archives", now);
        AddTranslation(translations, "bandDetail", "bandDetail.formedIn", lang, "\u0417\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u043E \u0443", now);

        // albumDetail
        AddTranslation(translations, "albumDetail", "albumDetail.backToAlbums", lang, "\u041D\u0430\u0437\u0430\u0434 \u0434\u043E \u0430\u043B\u044C\u0431\u043E\u043C\u0456\u0432", now);
        AddTranslation(translations, "albumDetail", "albumDetail.availableAt", lang, "\u0414\u043E\u0441\u0442\u0443\u043F\u043D\u043E \u0432 {count} \u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0430\u0445", now);
        AddTranslation(translations, "albumDetail", "albumDetail.availableAtOne", lang, "\u0414\u043E\u0441\u0442\u0443\u043F\u043D\u043E \u0432 1 \u043C\u0430\u0433\u0430\u0437\u0438\u043D\u0456", now);
        AddTranslation(translations, "albumDetail", "albumDetail.buy", lang, "\u041A\u0443\u043F\u0438\u0442\u0438", now);
        AddTranslation(translations, "albumDetail", "albumDetail.moreByBand", lang, "\u0406\u043D\u0448\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u0433\u0443\u0440\u0442\u0443 {bandName}", now);
        AddTranslation(translations, "albumDetail", "albumDetail.notFound", lang, "\u0410\u043B\u044C\u0431\u043E\u043C \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "albumDetail", "albumDetail.error", lang, "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u0437\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0438\u0442\u0438 \u0456\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0456\u044E \u043F\u0440\u043E \u0430\u043B\u044C\u0431\u043E\u043C.", now);
        AddTranslation(translations, "albumDetail", "albumDetail.label", lang, "\u041B\u0435\u0439\u0431\u043B", now);
        AddTranslation(translations, "albumDetail", "albumDetail.press", lang, "\u0422\u0438\u0440\u0430\u0436", now);
        AddTranslation(translations, "albumDetail", "albumDetail.shipsFrom", lang, "\u0414\u043E\u0441\u0442\u0430\u0432\u043A\u0430 \u0437", now);
        AddTranslation(translations, "albumDetail", "albumDetail.listenOnBandcamp", lang, "\u0421\u043B\u0443\u0445\u0430\u0442\u0438 \u043D\u0430 Bandcamp", now);
        AddTranslation(translations, "albumDetail", "albumDetail.alsoAvailableAs", lang, "\u0422\u0430\u043A\u043E\u0436 \u0434\u043E\u0441\u0442\u0443\u043F\u043D\u043E \u044F\u043A", now);

        // priceHistory
        AddTranslation(translations, "priceHistory", "priceHistory.title", lang, "\u0406\u0441\u0442\u043E\u0440\u0456\u044F \u0446\u0456\u043D", now);
        AddTranslation(translations, "priceHistory", "priceHistory.noData", lang, "\u0406\u0441\u0442\u043E\u0440\u0456\u044F \u0446\u0456\u043D \u043D\u0435\u0434\u043E\u0441\u0442\u0443\u043F\u043D\u0430", now);

        // rating
        AddTranslation(translations, "rating", "rating.average", lang, "{count} \u043E\u0446\u0456\u043D\u043E\u043A", now);
        AddTranslation(translations, "rating", "rating.averageOne", lang, "1 \u043E\u0446\u0456\u043D\u043A\u0430", now);
        AddTranslation(translations, "rating", "rating.loginToRate", lang, "\u0423\u0432\u0456\u0439\u0434\u0456\u0442\u044C, \u0449\u043E\u0431 \u043E\u0446\u0456\u043D\u0438\u0442\u0438", now);
        AddTranslation(translations, "rating", "rating.yourRating", lang, "\u0412\u0430\u0448\u0430 \u043E\u0446\u0456\u043D\u043A\u0430", now);

        // notFound
        AddTranslation(translations, "notFound", "notFound.title", lang, "404", now);
        AddTranslation(translations, "notFound", "notFound.heading", lang, "\u0421\u0442\u043E\u0440\u0456\u043D\u043A\u0443 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E", now);
        AddTranslation(translations, "notFound", "notFound.message", lang, "\u0421\u0442\u043E\u0440\u0456\u043D\u043A\u0430, \u044F\u043A\u0443 \u0432\u0438 \u0448\u0443\u043A\u0430\u0454\u0442\u0435, \u043D\u0435 \u0456\u0441\u043D\u0443\u0454 \u0430\u0431\u043E \u0431\u0443\u043B\u0430 \u043F\u0435\u0440\u0435\u043C\u0456\u0449\u0435\u043D\u0430.", now);
        AddTranslation(translations, "notFound", "notFound.backHome", lang, "\u041D\u0430 \u0433\u043E\u043B\u043E\u0432\u043D\u0443", now);
        AddTranslation(translations, "notFound", "notFound.backAlbums", lang, "\u041F\u0435\u0440\u0435\u0433\u043B\u044F\u043D\u0443\u0442\u0438 \u0430\u043B\u044C\u0431\u043E\u043C\u0438", now);

        // footer
        AddTranslation(translations, "footer", "footer.rights", lang, "\u0412\u0441\u0456 \u043F\u0440\u0430\u0432\u0430 \u0437\u0430\u0445\u0438\u0449\u0435\u043D\u0456.", now);
        AddTranslation(translations, "footer", "footer.suggestDistributor", lang, "\u0417\u0430\u043F\u0440\u043E\u043F\u043E\u043D\u0443\u0432\u0430\u0442\u0438 \u0434\u0438\u0441\u0442\u0440\u0438\u0431'\u044E\u0442\u043E\u0440\u0430", now);

        // notifications
        AddTranslation(translations, "notifications", "notifications.title", lang, "\u0421\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "notifications", "notifications.noNotifications", lang, "\u0421\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u044C \u043F\u043E\u043A\u0438 \u043D\u0435\u043C\u0430\u0454", now);
        AddTranslation(translations, "notifications", "notifications.markAllRead", lang, "\u041F\u043E\u0437\u043D\u0430\u0447\u0438\u0442\u0438 \u0432\u0441\u0456 \u044F\u043A \u043F\u0440\u043E\u0447\u0438\u0442\u0430\u043D\u0456", now);

        // watch
        AddTranslation(translations, "watch", "watch.watchTooltip", lang, "\u041E\u0442\u0440\u0438\u043C\u0443\u0432\u0430\u0442\u0438 \u0441\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u043D\u044F \u043F\u0440\u043E \u0437\u043D\u0438\u0436\u0435\u043D\u043D\u044F \u0446\u0456\u043D\u0438 \u0442\u0430 \u043F\u043E\u0432\u0435\u0440\u043D\u0435\u043D\u043D\u044F \u0432 \u043F\u0440\u043E\u0434\u0430\u0436", now);
        AddTranslation(translations, "watch", "watch.unwatchTooltip", lang, "\u041F\u0440\u0438\u043F\u0438\u043D\u0438\u0442\u0438 \u0441\u0442\u0435\u0436\u0438\u0442\u0438 \u0437\u0430 \u0446\u0438\u043C \u0430\u043B\u044C\u0431\u043E\u043C\u043E\u043C", now);

        // calendar
        AddTranslation(translations, "calendar", "calendar.title", lang, "\u041A\u0430\u043B\u0435\u043D\u0434\u0430\u0440 \u0440\u0435\u043B\u0456\u0437\u0456\u0432", now);
        AddTranslation(translations, "calendar", "calendar.description", lang, "\u041C\u0430\u0439\u0431\u0443\u0442\u043D\u0456 \u043F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F \u0442\u0430 \u043D\u0435\u0449\u043E\u0434\u0430\u0432\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 \u0443\u043A\u0440\u0430\u0457\u043D\u0441\u044C\u043A\u0438\u0445 \u043C\u0435\u0442\u0430\u043B-\u0433\u0443\u0440\u0442\u0456\u0432.", now);
        AddTranslation(translations, "calendar", "calendar.preOrders", lang, "\u041F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F", now);
        AddTranslation(translations, "calendar", "calendar.recentReleases", lang, "\u041D\u0435\u0449\u043E\u0434\u0430\u0432\u043D\u0456 \u0440\u0435\u043B\u0456\u0437\u0438 (\u043E\u0441\u0442\u0430\u043D\u043D\u0456 30 \u0434\u043D\u0456\u0432)", now);
        AddTranslation(translations, "calendar", "calendar.noReleases", lang, "\u0420\u0435\u043B\u0456\u0437\u0456\u0432 \u0437\u0430 \u043E\u0431\u0440\u0430\u043D\u0438\u043C\u0438 \u0444\u0456\u043B\u044C\u0442\u0440\u0430\u043C\u0438 \u043D\u0435 \u0437\u043D\u0430\u0439\u0434\u0435\u043D\u043E.", now);
        AddTranslation(translations, "calendar", "calendar.preOrder", lang, "\u041F\u0435\u0440\u0435\u0434\u0437\u0430\u043C\u043E\u0432\u043B\u0435\u043D\u043D\u044F", now);

        // telegram
        AddTranslation(translations, "telegram", "telegram.title", lang, "\u0421\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u043D\u044F \u0432 Telegram", now);
        AddTranslation(translations, "telegram", "telegram.description", lang, "\u041F\u0456\u0434\u043A\u043B\u044E\u0447\u0456\u0442\u044C Telegram \u0449\u043E\u0431 \u043E\u0442\u0440\u0438\u043C\u0443\u0432\u0430\u0442\u0438 \u043C\u0438\u0442\u0442\u0454\u0432\u0456 \u0441\u043F\u043E\u0432\u0456\u0449\u0435\u043D\u043D\u044F \u043F\u0440\u043E \u0437\u043D\u0438\u0436\u0435\u043D\u043D\u044F \u0446\u0456\u043D, \u043F\u043E\u0432\u0435\u0440\u043D\u0435\u043D\u043D\u044F \u0432 \u043F\u0440\u043E\u0434\u0430\u0436 \u0442\u0430 \u043D\u043E\u0432\u0456 \u0440\u0435\u043B\u0456\u0437\u0438.", now);
        AddTranslation(translations, "telegram", "telegram.linked", lang, "Telegram \u0430\u043A\u0430\u0443\u043D\u0442 \u043F\u0456\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u043E", now);
        AddTranslation(translations, "telegram", "telegram.unlink", lang, "\u0412\u0456\u0434\u043A\u043B\u044E\u0447\u0438\u0442\u0438", now);
        AddTranslation(translations, "telegram", "telegram.linkButton", lang, "\u041F\u0456\u0434\u043A\u043B\u044E\u0447\u0438\u0442\u0438 Telegram", now);
        AddTranslation(translations, "telegram", "telegram.sendCommand", lang, "\u041D\u0430\u0434\u0456\u0448\u043B\u0456\u0442\u044C \u0446\u044E \u043A\u043E\u043C\u0430\u043D\u0434\u0443 \u0431\u043E\u0442\u0443:", now);
        AddTranslation(translations, "telegram", "telegram.copy", lang, "\u041A\u043E\u043F\u0456\u044E\u0432\u0430\u0442\u0438", now);
        AddTranslation(translations, "telegram", "telegram.openBot", lang, "\u0412\u0456\u0434\u043A\u0440\u0438\u0442\u0438 \u0431\u043E\u0442\u0430", now);
    }
}
