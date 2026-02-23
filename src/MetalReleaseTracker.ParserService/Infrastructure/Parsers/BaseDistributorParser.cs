using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public abstract class BaseDistributorParser : IListingParser, IAlbumDetailParser
{
    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;
    private readonly Random _random = new();
    private GeneralParserSettings? _cachedSettings;
    private Queue<(string Url, AlbumMediaType MediaType)> _pendingCategoryUrls = new();
    private bool _categoryQueueInitialized;

    protected BaseDistributorParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISettingsService settingsService,
        ILogger logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _settingsService = settingsService;
        _logger = logger;
    }

    public abstract DistributorCode DistributorCode { get; }

    protected abstract string[] CatalogueUrls { get; }

    protected abstract string ParserName { get; }

    protected virtual AlbumMediaType[] CategoryMediaTypes =>
        [AlbumMediaType.CD, AlbumMediaType.LP, AlbumMediaType.Tape];

    protected AlbumMediaType? CurrentCategoryMediaType { get; private set; }

    public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var pageUrl = url;

            if (!_categoryQueueInitialized)
            {
                pageUrl = InitializeCategoryQueue(url);
                _categoryQueueInitialized = true;
            }

            _logger.LogInformation("Crawling {ParserName} page: {Url}.", ParserName, pageUrl);

            var htmlDocument = await LoadHtmlDocument(pageUrl, cancellationToken);
            var listings = ParseListingsFromPage(htmlDocument);

            _logger.LogInformation("Parsed {Count} products from page.", listings.Count);

            var nextPageUrl = ResolveNextPageUrl(htmlDocument);

            return new ListingPageResult
            {
                Listings = listings,
                NextPageUrl = nextPageUrl
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (!IsOwnException(exception))
        {
            _logger.LogError(exception, "Error during {ParserName} catalogue crawl for URL: {Url}.", ParserName, url);
            throw CreateParserException($"Failed to crawl {ParserName} catalogue: {url}", exception);
        }
    }

    public async Task<AlbumParsedEvent> ParseAlbumDetailAsync(string detailUrl, CancellationToken cancellationToken)
    {
        try
        {
            _cachedSettings ??= await _settingsService.GetGeneralParserSettingsAsync(cancellationToken);
            await ParserHelper.DelayBetweenRequestsAsync(_cachedSettings, _random, cancellationToken);
            return await ParseAlbumDetails(detailUrl, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (!IsOwnException(exception))
        {
            _logger.LogError(exception, "Error during {ParserName} detail parse for URL: {Url}.", ParserName, detailUrl);
            throw CreateParserException($"Failed to parse {ParserName} detail page: {detailUrl}", exception);
        }
    }

    protected abstract List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument);

    protected abstract Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken);

    protected abstract HtmlNode? FindNextPageLink(HtmlDocument htmlDocument);

    protected abstract Exception CreateParserException(string message, Exception? innerException = null);

    protected abstract bool IsOwnException(Exception exception);

    protected async Task<HtmlDocument> LoadHtmlDocument(string url, CancellationToken cancellationToken)
    {
        return await ParserHelper.LoadHtmlDocumentOrThrowAsync(
            _htmlDocumentLoader,
            url,
            _logger,
            message => CreateParserException(message),
            cancellationToken);
    }

    private string InitializeCategoryQueue(string initialUrl)
    {
        var entries = new List<(string Url, AlbumMediaType MediaType)>();

        for (var i = 0; i < CatalogueUrls.Length; i++)
        {
            var mediaType = i < CategoryMediaTypes.Length ? CategoryMediaTypes[i] : AlbumMediaType.CD;

            if (string.Equals(CatalogueUrls[i], initialUrl, StringComparison.OrdinalIgnoreCase))
            {
                CurrentCategoryMediaType = mediaType;
            }
            else
            {
                entries.Add((CatalogueUrls[i], mediaType));
            }
        }

        _pendingCategoryUrls = new Queue<(string Url, AlbumMediaType MediaType)>(entries);

        _logger.LogInformation(
            "Initialized {ParserName} category queue. Starting with {Url}, {Remaining} categories remaining.",
            ParserName,
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = FindNextPageLink(htmlDocument);

        if (nextPageLink != null)
        {
            var nextUrl = HtmlEntity.DeEntitize(nextPageLink.GetAttributeValue("href", string.Empty).Trim());
            if (!string.IsNullOrEmpty(nextUrl))
            {
                return TransformNextPageUrl(nextUrl);
            }
        }

        if (_pendingCategoryUrls.Count > 0)
        {
            var (nextUrl, mediaType) = _pendingCategoryUrls.Dequeue();
            CurrentCategoryMediaType = mediaType;
            _logger.LogInformation("Moving to next category: {Url}.", nextUrl);
            return nextUrl;
        }

        _logger.LogInformation("All {ParserName} categories crawled.", ParserName);
        return null;
    }

    protected virtual string TransformNextPageUrl(string url)
    {
        return url;
    }
}
