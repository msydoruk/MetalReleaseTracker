using System.Text;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class SitemapService : ISitemapService
{
    private const string FallbackBaseUrl = "https://metal-release.com";
    private const string CacheKey = "sitemap_xml";

    private static readonly (string Path, string Priority, string ChangeFreq)[] StaticPages =
    [
        ("/", "1.0", "daily"),
        ("/albums", "0.9", "daily"),
        ("/bands", "0.8", "weekly"),
        ("/distributors", "0.7", "weekly"),
        ("/news", "0.6", "weekly"),
        ("/about", "0.5", "monthly"),
        ("/reviews", "0.5", "weekly"),
        ("/changelog", "0.6", "daily"),
    ];

    private readonly IAlbumRepository _albumRepository;
    private readonly IBandRepository _bandRepository;
    private readonly IDistributorsRepository _distributorsRepository;
    private readonly IAdminSettingsService _settingsService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SitemapService> _logger;

    public SitemapService(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IDistributorsRepository distributorsRepository,
        IAdminSettingsService settingsService,
        IMemoryCache memoryCache,
        ILogger<SitemapService> logger)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
        _distributorsRepository = distributorsRepository;
        _settingsService = settingsService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string> GenerateSitemapAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(CacheKey, out string? cached) && cached != null)
        {
            return cached;
        }

        var xml = await BuildSitemapXmlAsync(cancellationToken);
        _memoryCache.Set(CacheKey, xml, TimeSpan.FromHours(1));

        return xml;
    }

    private async Task<string> BuildSitemapXmlAsync(CancellationToken cancellationToken)
    {
        var baseUrl = await _settingsService.GetStringSettingAsync(
            SettingCategories.Seo,
            SettingKeys.Seo.SiteUrl,
            FallbackBaseUrl,
            cancellationToken);

        var albumSlugs = await _albumRepository.GetAllAlbumSlugsAsync(cancellationToken);
        var bandSlugs = await _bandRepository.GetAllBandSlugsAsync(cancellationToken);
        var distributors = await _distributorsRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation(
            "Generating sitemap with {AlbumCount} albums, {BandCount} bands, and {DistributorCount} distributors",
            albumSlugs.Count,
            bandSlugs.Count,
            distributors.Count);

        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var (path, priority, changeFreq) in StaticPages)
        {
            builder.AppendLine("  <url>");
            builder.Append("    <loc>").Append(baseUrl).Append(path).AppendLine("</loc>");
            builder.Append("    <changefreq>").Append(changeFreq).AppendLine("</changefreq>");
            builder.Append("    <priority>").Append(priority).AppendLine("</priority>");
            builder.AppendLine("  </url>");
        }

        foreach (var band in bandSlugs)
        {
            builder.AppendLine("  <url>");
            builder.Append("    <loc>").Append(baseUrl).Append("/bands/").Append(band.Slug).AppendLine("</loc>");
            builder.AppendLine("    <changefreq>weekly</changefreq>");
            builder.AppendLine("    <priority>0.7</priority>");
            builder.AppendLine("  </url>");
        }

        foreach (var distributor in distributors)
        {
            if (string.IsNullOrEmpty(distributor.Slug))
            {
                continue;
            }

            builder.AppendLine("  <url>");
            builder.Append("    <loc>").Append(baseUrl).Append("/distributors/").Append(distributor.Slug).AppendLine("</loc>");
            builder.AppendLine("    <changefreq>monthly</changefreq>");
            builder.AppendLine("    <priority>0.6</priority>");
            builder.AppendLine("  </url>");
        }

        foreach (var album in albumSlugs)
        {
            builder.AppendLine("  <url>");
            builder.Append("    <loc>").Append(baseUrl).Append("/albums/").Append(album.Slug).AppendLine("</loc>");
            builder.Append("    <lastmod>").Append(album.LastModified.ToString("yyyy-MM-dd")).AppendLine("</lastmod>");
            builder.AppendLine("    <changefreq>weekly</changefreq>");
            builder.AppendLine("    <priority>0.6</priority>");
            builder.AppendLine("  </url>");
        }

        builder.AppendLine("</urlset>");

        return builder.ToString();
    }
}
