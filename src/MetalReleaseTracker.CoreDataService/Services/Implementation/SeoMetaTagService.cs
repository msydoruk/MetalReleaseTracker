using System.Text.RegularExpressions;
using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public partial class SeoMetaTagService : ISeoMetaTagService
{
    private const string FallbackSiteName = "Metal Release Tracker";
    private const string FallbackSiteUrl = "https://metal-release.com";
    private const string FallbackDescription = "Track and buy physical releases (vinyl, CD, tape) of Ukrainian metal bands from foreign distributors and labels. One catalog, direct links to stores.";
    private const string FallbackOgImage = "https://metal-release.com/logo512.png";
    private const string SeoSettingsCacheKey = "seo_settings";

    private static readonly Dictionary<string, SeoStaticPageMeta> StaticPagePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/"] = new SeoStaticPageMeta("home"),
        ["/albums"] = new SeoStaticPageMeta("albums"),
        ["/bands"] = new SeoStaticPageMeta("bands"),
        ["/distributors"] = new SeoStaticPageMeta("distributors"),
        ["/news"] = new SeoStaticPageMeta("news"),
        ["/about"] = new SeoStaticPageMeta("about"),
        ["/reviews"] = new SeoStaticPageMeta("reviews"),
        ["/changelog"] = new SeoStaticPageMeta("changelog"),
        ["/calendar"] = new SeoStaticPageMeta("calendar"),
    };

    private static readonly object TemplateLock = new();

    private static string? _indexHtmlTemplate;

    private readonly IAlbumRepository _albumRepository;
    private readonly IBandRepository _bandRepository;
    private readonly IAdminSettingsService _settingsService;
    private readonly CoreDataServiceDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SeoMetaTagService> _logger;

    public SeoMetaTagService(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IAdminSettingsService settingsService,
        CoreDataServiceDbContext context,
        IMemoryCache memoryCache,
        IWebHostEnvironment environment,
        ILogger<SeoMetaTagService> logger)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
        _settingsService = settingsService;
        _context = context;
        _memoryCache = memoryCache;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> GetHtmlWithMetaTags(string path, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"seo_html:{path}";
        if (_memoryCache.TryGetValue(cacheKey, out string? cached) && cached != null)
        {
            return cached;
        }

        var seoSettings = await GetSeoSettingsAsync(cancellationToken);
        var template = GetTemplate();
        var metaTags = await ResolveMetaTagsAsync(path, seoSettings, cancellationToken);
        metaTags.CanonicalUrl = $"{seoSettings.SiteUrl}{path}";

        var html = InjectMetaTags(template, metaTags, seoSettings);
        _memoryCache.Set(cacheKey, html, TimeSpan.FromHours(1));

        return html;
    }

    private async Task<SeoSettings> GetSeoSettingsAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(SeoSettingsCacheKey, out SeoSettings? cached) && cached != null)
        {
            return cached;
        }

        var settings = new SeoSettings
        {
            SiteName = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.SiteName, FallbackSiteName, cancellationToken),
            SiteUrl = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.SiteUrl, FallbackSiteUrl, cancellationToken),
            DefaultDescription = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.DefaultMetaDescription, FallbackDescription, cancellationToken),
            DefaultOgImage = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.DefaultOgImage, FallbackOgImage, cancellationToken),
            OrganizationName = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.OrganizationName, FallbackSiteName, cancellationToken),
            OrganizationLogoUrl = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.OrganizationLogoUrl, FallbackOgImage, cancellationToken),
            GoogleSiteVerification = await _settingsService.GetStringSettingAsync(
                SettingCategories.Seo, SettingKeys.Seo.GoogleSiteVerification, string.Empty, cancellationToken),
        };

        _memoryCache.Set(SeoSettingsCacheKey, settings, TimeSpan.FromMinutes(30));

        return settings;
    }

    private async Task<SeoMetaTagsDto> ResolveMetaTagsAsync(string path, SeoSettings seoSettings, CancellationToken cancellationToken)
    {
        var trimmedPath = path.TrimEnd('/');

        if (StaticPagePaths.TryGetValue(trimmedPath, out var staticPage))
        {
            return await ResolveStaticPageMetaAsync(staticPage.PageKey, seoSettings, cancellationToken);
        }

        if (trimmedPath.StartsWith("/albums/", StringComparison.OrdinalIgnoreCase))
        {
            var slug = trimmedPath["/albums/".Length..];
            return await ResolveAlbumMetaAsync(slug, seoSettings, cancellationToken);
        }

        if (trimmedPath.StartsWith("/bands/", StringComparison.OrdinalIgnoreCase))
        {
            var slug = trimmedPath["/bands/".Length..];
            return await ResolveBandMetaAsync(slug, seoSettings, cancellationToken);
        }

        return new SeoMetaTagsDto
        {
            Title = $"{seoSettings.SiteName} - {seoSettings.DefaultDescription}",
            Description = seoSettings.DefaultDescription,
            ImageUrl = seoSettings.DefaultOgImage,
        };
    }

    private async Task<SeoMetaTagsDto> ResolveStaticPageMetaAsync(string pageKey, SeoSettings seoSettings, CancellationToken cancellationToken)
    {
        var titleKey = $"pageMeta.{pageKey}Title";
        var descriptionKey = $"pageMeta.{pageKey}Description";

        var title = await GetTranslationAsync(titleKey, cancellationToken);
        var description = await GetTranslationAsync(descriptionKey, cancellationToken);

        if (string.IsNullOrEmpty(title))
        {
            title = seoSettings.SiteName;
        }

        return new SeoMetaTagsDto
        {
            Title = title,
            Description = description ?? seoSettings.DefaultDescription,
            ImageUrl = seoSettings.DefaultOgImage,
        };
    }

    private async Task<SeoMetaTagsDto> ResolveAlbumMetaAsync(string slug, SeoSettings seoSettings, CancellationToken cancellationToken)
    {
        try
        {
            var album = await _albumRepository.GetBySlugAsync(slug, cancellationToken);
            if (album == null)
            {
                return CreateDefaultMeta(seoSettings);
            }

            var bandName = album.Band?.Name ?? "Unknown";
            var albumName = album.CanonicalTitle ?? album.Name;

            var seoTranslation = await _context.AlbumTranslations
                .AsNoTracking()
                .Where(t => t.AlbumId == album.Id && t.LanguageCode == "en")
                .Select(t => new { t.SeoTitle, t.SeoDescription, t.SeoKeywords })
                .FirstOrDefaultAsync(cancellationToken);

            var title = seoTranslation?.SeoTitle ?? $"{albumName} by {bandName} | {seoSettings.SiteName}";

            if (seoTranslation?.SeoDescription != null)
            {
                return new SeoMetaTagsDto
                {
                    Title = title,
                    Description = seoTranslation.SeoDescription,
                    OgType = "music.album",
                    Keywords = seoTranslation.SeoKeywords,
                };
            }

            var descriptionParts = new List<string> { $"Buy {albumName} by {bandName}" };

            var year = album.OriginalYear?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(year))
            {
                descriptionParts.Add($"({year})");
            }

            var genre = album.Genre ?? string.Empty;
            if (!string.IsNullOrEmpty(genre))
            {
                descriptionParts.Add($"- {genre}");
            }

            var media = album.Media?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(media))
            {
                descriptionParts.Add($"- {media}");
            }

            descriptionParts.Add($"from EUR {album.Price:F2}");

            return new SeoMetaTagsDto
            {
                Title = title,
                Description = string.Join(" ", descriptionParts),
                OgType = "music.album",
                Keywords = seoTranslation?.SeoKeywords,
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to resolve album meta for slug: {Slug}", slug);
            return CreateDefaultMeta(seoSettings);
        }
    }

    private async Task<SeoMetaTagsDto> ResolveBandMetaAsync(string slug, SeoSettings seoSettings, CancellationToken cancellationToken)
    {
        try
        {
            var band = await _bandRepository.GetBySlugAsync(slug, cancellationToken);
            if (band == null)
            {
                return CreateDefaultMeta(seoSettings);
            }

            var seoTranslation = await _context.BandTranslations
                .AsNoTracking()
                .Where(t => t.BandId == band.Id && t.LanguageCode == "en")
                .Select(t => new { t.SeoTitle, t.SeoDescription, t.SeoKeywords })
                .FirstOrDefaultAsync(cancellationToken);

            var title = seoTranslation?.SeoTitle ?? $"{band.Name} | {seoSettings.SiteName}";

            if (seoTranslation?.SeoDescription != null)
            {
                return new SeoMetaTagsDto
                {
                    Title = title,
                    Description = seoTranslation.SeoDescription,
                    Keywords = seoTranslation.SeoKeywords,
                };
            }

            var genre = band.Genre ?? "Metal";
            var description = $"{band.Name} - {genre} from Ukraine. Physical releases available from foreign distributors.";

            return new SeoMetaTagsDto
            {
                Title = title,
                Description = description,
                Keywords = seoTranslation?.SeoKeywords,
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to resolve band meta for slug: {Slug}", slug);
            return CreateDefaultMeta(seoSettings);
        }
    }

    private async Task<string?> GetTranslationAsync(string key, CancellationToken cancellationToken)
    {
        var cacheKey = $"seo_translation:{key}";
        if (_memoryCache.TryGetValue(cacheKey, out string? cached))
        {
            return cached;
        }

        try
        {
            var translation = await _context.Translations
                .AsNoTracking()
                .Where(translation => translation.Key == key && translation.Language == "en")
                .Select(translation => translation.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(translation))
            {
                return null;
            }

            _memoryCache.Set(cacheKey, translation, TimeSpan.FromHours(1));
            return translation;
        }
        catch
        {
            return null;
        }
    }

    private string GetTemplate()
    {
        if (_indexHtmlTemplate != null)
        {
            return _indexHtmlTemplate;
        }

        lock (TemplateLock)
        {
            if (_indexHtmlTemplate != null)
            {
                return _indexHtmlTemplate;
            }

            var indexPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "index.html");
            _indexHtmlTemplate = File.Exists(indexPath)
                ? File.ReadAllText(indexPath)
                : $"<!DOCTYPE html><html><head><title>{FallbackSiteName}</title></head><body><div id=\"root\"></div></body></html>";
        }

        return _indexHtmlTemplate;
    }

    private static string InjectMetaTags(string template, SeoMetaTagsDto meta, SeoSettings seoSettings)
    {
        var html = template;

        html = TitleRegex().Replace(html, $"<title>{EscapeHtml(meta.Title)}</title>");

        html = DescriptionRegex().Replace(html,
            $"<meta name=\"description\" content=\"{EscapeHtml(meta.Description)}\" />");

        html = CanonicalRegex().Replace(html,
            $"<link rel=\"canonical\" href=\"{meta.CanonicalUrl}\" />");

        html = OgTitleRegex().Replace(html,
            $"<meta property=\"og:title\" content=\"{EscapeHtml(meta.Title)}\" />");
        html = OgDescriptionRegex().Replace(html,
            $"<meta property=\"og:description\" content=\"{EscapeHtml(meta.Description)}\" />");
        html = OgUrlRegex().Replace(html,
            $"<meta property=\"og:url\" content=\"{meta.CanonicalUrl}\" />");

        if (!string.IsNullOrEmpty(meta.ImageUrl))
        {
            html = OgImageRegex().Replace(html,
                $"<meta property=\"og:image\" content=\"{meta.ImageUrl}\" />");
        }

        html = TwitterTitleRegex().Replace(html,
            $"<meta name=\"twitter:title\" content=\"{EscapeHtml(meta.Title)}\" />");
        html = TwitterDescriptionRegex().Replace(html,
            $"<meta name=\"twitter:description\" content=\"{EscapeHtml(meta.Description)}\" />");

        if (!string.IsNullOrEmpty(meta.ImageUrl))
        {
            html = TwitterImageRegex().Replace(html,
                $"<meta name=\"twitter:image\" content=\"{meta.ImageUrl}\" />");
        }

        if (!string.IsNullOrEmpty(seoSettings.GoogleSiteVerification))
        {
            var verificationTag = $"<meta name=\"google-site-verification\" content=\"{seoSettings.GoogleSiteVerification}\" />";
            html = html.Replace("</head>", $"    {verificationTag}\n</head>");
        }

        html = WebSiteJsonLdRegex().Replace(html, BuildWebSiteJsonLd(seoSettings));
        html = OrganizationJsonLdRegex().Replace(html, BuildOrganizationJsonLd(seoSettings));

        return html;
    }

    private static SeoMetaTagsDto CreateDefaultMeta(SeoSettings seoSettings)
    {
        return new SeoMetaTagsDto
        {
            Title = seoSettings.SiteName,
            Description = seoSettings.DefaultDescription,
            ImageUrl = seoSettings.DefaultOgImage,
        };
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string BuildWebSiteJsonLd(SeoSettings seoSettings)
    {
        return $$"""
            <script type="application/ld+json">
            {
              "@context": "https://schema.org",
              "@type": "WebSite",
              "name": "{{EscapeJsonString(seoSettings.SiteName)}}",
              "url": "{{seoSettings.SiteUrl}}/",
              "description": "{{EscapeJsonString(seoSettings.DefaultDescription)}}",
              "inLanguage": ["en", "uk"],
              "potentialAction": {
                "@type": "SearchAction",
                "target": "{{seoSettings.SiteUrl}}/albums?search={search_term_string}",
                "query-input": "required name=search_term_string"
              }
            }
            </script>
            """;
    }

    private static string BuildOrganizationJsonLd(SeoSettings seoSettings)
    {
        return $$"""
            <script type="application/ld+json">
            {
              "@context": "https://schema.org",
              "@type": "Organization",
              "name": "{{EscapeJsonString(seoSettings.OrganizationName)}}",
              "url": "{{seoSettings.SiteUrl}}/",
              "logo": "{{seoSettings.OrganizationLogoUrl}}"
            }
            </script>
            """;
    }

    private static string EscapeJsonString(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    [GeneratedRegex(@"<title>[^<]*</title>")]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<meta\s+name=""description""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex DescriptionRegex();

    [GeneratedRegex(@"<link\s+rel=""canonical""\s+href=""[^""]*""\s*/?>")]
    private static partial Regex CanonicalRegex();

    [GeneratedRegex(@"<meta\s+property=""og:title""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex OgTitleRegex();

    [GeneratedRegex(@"<meta\s+property=""og:description""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex OgDescriptionRegex();

    [GeneratedRegex(@"<meta\s+property=""og:url""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex OgUrlRegex();

    [GeneratedRegex(@"<meta\s+property=""og:image""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex OgImageRegex();

    [GeneratedRegex(@"<meta\s+name=""twitter:title""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex TwitterTitleRegex();

    [GeneratedRegex(@"<meta\s+name=""twitter:description""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex TwitterDescriptionRegex();

    [GeneratedRegex(@"<meta\s+name=""twitter:image""\s+content=""[^""]*""\s*/?>")]
    private static partial Regex TwitterImageRegex();

    [GeneratedRegex(@"<script\s+type=""application/ld\+json"">\s*\{[^}]*""@type""\s*:\s*""WebSite""[^}]*\}\s*</script>", RegexOptions.Singleline)]
    private static partial Regex WebSiteJsonLdRegex();

    [GeneratedRegex(@"<script\s+type=""application/ld\+json"">\s*\{[^}]*""@type""\s*:\s*""Organization""[^}]*\}\s*</script>", RegexOptions.Singleline)]
    private static partial Regex OrganizationJsonLdRegex();

    private sealed class SeoSettings
    {
        public string SiteName { get; init; } = FallbackSiteName;

        public string SiteUrl { get; init; } = FallbackSiteUrl;

        public string DefaultDescription { get; init; } = FallbackDescription;

        public string DefaultOgImage { get; init; } = FallbackOgImage;

        public string OrganizationName { get; init; } = FallbackSiteName;

        public string OrganizationLogoUrl { get; init; } = FallbackOgImage;

        public string? GoogleSiteVerification { get; init; }
    }

    private sealed class SeoStaticPageMeta
    {
        public SeoStaticPageMeta(string pageKey)
        {
            PageKey = pageKey;
        }

        public string PageKey { get; }
    }
}
