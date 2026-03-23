using System.Text.RegularExpressions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public partial class SeoMetaTagService : ISeoMetaTagService
{
    private const string BaseUrl = "https://metal-release.com";
    private const string DefaultTitle = "Metal Release Tracker - Ukrainian Metal Releases from Foreign Distributors";
    private const string DefaultDescription = "Track and buy physical releases (vinyl, CD, tape) of Ukrainian metal bands from foreign distributors and labels. One catalog, direct links to stores.";
    private const string DefaultImage = "https://metal-release.com/logo512.png";

    private static readonly Dictionary<string, SeoMetaTagsDto> StaticPageMeta = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/"] = new SeoMetaTagsDto
        {
            Title = DefaultTitle,
            Description = DefaultDescription,
            ImageUrl = DefaultImage,
        },
        ["/albums"] = new SeoMetaTagsDto
        {
            Title = "Browse Albums | Metal Release Tracker",
            Description = "Browse physical releases of Ukrainian metal bands. Filter by genre, format, distributor. Vinyl, CD, and cassette from labels worldwide.",
        },
        ["/bands"] = new SeoMetaTagsDto
        {
            Title = "Ukrainian Metal Bands | Metal Release Tracker",
            Description = "Discover Ukrainian metal bands with foreign releases. Black metal, death metal, folk metal and more.",
        },
        ["/distributors"] = new SeoMetaTagsDto
        {
            Title = "Distributors | Metal Release Tracker",
            Description = "Foreign distributors and labels that carry Ukrainian metal releases. Compare prices and availability.",
        },
        ["/news"] = new SeoMetaTagsDto
        {
            Title = "News | Metal Release Tracker",
            Description = "Latest updates about Ukrainian metal releases, new distributors, and platform features.",
        },
        ["/about"] = new SeoMetaTagsDto
        {
            Title = "About | Metal Release Tracker",
            Description = "Metal Release Tracker aggregates physical releases of Ukrainian metal bands from foreign distributors and labels.",
        },
        ["/reviews"] = new SeoMetaTagsDto
        {
            Title = "Reviews | Metal Release Tracker",
            Description = "User reviews and feedback about Metal Release Tracker.",
        },
        ["/changelog"] = new SeoMetaTagsDto
        {
            Title = "Changelog | Metal Release Tracker",
            Description = "Recent album additions, price changes, and catalog updates.",
        },
    };

    private static readonly object TemplateLock = new();

    private static string? _indexHtmlTemplate;

    private readonly IAlbumRepository _albumRepository;
    private readonly IBandRepository _bandRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SeoMetaTagService> _logger;

    public SeoMetaTagService(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IMemoryCache memoryCache,
        IWebHostEnvironment environment,
        ILogger<SeoMetaTagService> logger)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
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

        var template = GetTemplate();
        var metaTags = await ResolveMetaTagsAsync(path, cancellationToken);
        metaTags.CanonicalUrl = $"{BaseUrl}{path}";

        var html = InjectMetaTags(template, metaTags);
        _memoryCache.Set(cacheKey, html, TimeSpan.FromHours(1));

        return html;
    }

    private async Task<SeoMetaTagsDto> ResolveMetaTagsAsync(string path, CancellationToken cancellationToken)
    {
        if (StaticPageMeta.TryGetValue(path.TrimEnd('/'), out var staticMeta))
        {
            return staticMeta;
        }

        if (path.StartsWith("/albums/", StringComparison.OrdinalIgnoreCase))
        {
            var slug = path["/albums/".Length..].TrimEnd('/');
            return await ResolveAlbumMetaAsync(slug, cancellationToken);
        }

        if (path.StartsWith("/bands/", StringComparison.OrdinalIgnoreCase))
        {
            var slug = path["/bands/".Length..].TrimEnd('/');
            return await ResolveBandMetaAsync(slug, cancellationToken);
        }

        return new SeoMetaTagsDto
        {
            Title = DefaultTitle,
            Description = DefaultDescription,
            ImageUrl = DefaultImage,
        };
    }

    private async Task<SeoMetaTagsDto> ResolveAlbumMetaAsync(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var album = await _albumRepository.GetBySlugAsync(slug, cancellationToken);
            if (album == null)
            {
                return new SeoMetaTagsDto { Title = DefaultTitle, Description = DefaultDescription };
            }

            var bandName = album.Band?.Name ?? "Unknown";
            var albumName = album.CanonicalTitle ?? album.Name;
            var year = album.OriginalYear?.ToString() ?? string.Empty;
            var genre = album.Genre ?? string.Empty;
            var media = album.Media?.ToString() ?? string.Empty;

            var descriptionParts = new List<string> { $"Buy {albumName} by {bandName}" };
            if (!string.IsNullOrEmpty(year))
            {
                descriptionParts.Add($"({year})");
            }

            if (!string.IsNullOrEmpty(genre))
            {
                descriptionParts.Add($"- {genre}");
            }

            if (!string.IsNullOrEmpty(media))
            {
                descriptionParts.Add($"- {media}");
            }

            descriptionParts.Add($"from EUR {album.Price:F2}");

            return new SeoMetaTagsDto
            {
                Title = $"{albumName} by {bandName} | Metal Release Tracker",
                Description = string.Join(" ", descriptionParts),
                OgType = "music.album",
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to resolve album meta for slug: {Slug}", slug);
            return new SeoMetaTagsDto { Title = DefaultTitle, Description = DefaultDescription };
        }
    }

    private async Task<SeoMetaTagsDto> ResolveBandMetaAsync(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var band = await _bandRepository.GetBySlugAsync(slug, cancellationToken);
            if (band == null)
            {
                return new SeoMetaTagsDto { Title = DefaultTitle, Description = DefaultDescription };
            }

            var genre = band.Genre ?? "Metal";
            var description = $"{band.Name} - {genre} from Ukraine. Physical releases available from foreign distributors.";

            return new SeoMetaTagsDto
            {
                Title = $"{band.Name} | Metal Release Tracker",
                Description = description,
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to resolve band meta for slug: {Slug}", slug);
            return new SeoMetaTagsDto { Title = DefaultTitle, Description = DefaultDescription };
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
                : "<!DOCTYPE html><html><head><title>Metal Release Tracker</title></head><body><div id=\"root\"></div></body></html>";
        }

        return _indexHtmlTemplate;
    }

    private static string InjectMetaTags(string template, SeoMetaTagsDto meta)
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

        return html;
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
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
}
