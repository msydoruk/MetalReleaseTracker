using FileTypeChecker.Extensions;
using Flurl.Http;
using HtmlAgilityPack;
using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.ParserService.Infrastructure.Services;

public class BandPhotoSyncService : IBandPhotoSyncService
{
    private const string BandImagesFolder = "band-images";

    private readonly IAdminQueryRepository _adminQueryRepository;
    private readonly IFlareSolverrClient _flareSolverrClient;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserAgentProvider _userAgentProvider;
    private readonly ISettingsService _settingsService;
    private readonly ITopicProducer<BandPhotoSyncedEvent> _topicProducer;
    private readonly IParsingProgressTracker _progressTracker;
    private readonly ILogger<BandPhotoSyncService> _logger;
    private readonly Random _random = new();

    public BandPhotoSyncService(
        IAdminQueryRepository adminQueryRepository,
        IFlareSolverrClient flareSolverrClient,
        IFileStorageService fileStorageService,
        IUserAgentProvider userAgentProvider,
        ISettingsService settingsService,
        ITopicProducer<BandPhotoSyncedEvent> topicProducer,
        IParsingProgressTracker progressTracker,
        ILogger<BandPhotoSyncService> logger)
    {
        _adminQueryRepository = adminQueryRepository;
        _flareSolverrClient = flareSolverrClient;
        _fileStorageService = fileStorageService;
        _userAgentProvider = userAgentProvider;
        _settingsService = settingsService;
        _topicProducer = topicProducer;
        _progressTracker = progressTracker;
        _logger = logger;
    }

    public async Task SyncBandPhotosAsync(CancellationToken cancellationToken)
    {
        var bands = await _adminQueryRepository.GetBandPhotoMetadataAsync(cancellationToken);

        var uniqueBands = bands
            .GroupBy(band => band.MetalArchivesId)
            .Select(group => group.First())
            .ToList();

        _logger.LogInformation("Starting band photo sync for {Count} unique bands.", uniqueBands.Count);

        var runId = Guid.NewGuid();
        _progressTracker.StartRun(runId, ParsingJobType.BandPhotoSync, uniqueBands.Count);

        var settings = await _settingsService.GetBandReferenceSettingsAsync(cancellationToken);
        var sessionId = await _flareSolverrClient.CreateSessionAsync(cancellationToken);

        try
        {
            var flareSolverrResponse = await _flareSolverrClient.GetPageAsync(
                settings.MetalArchivesBaseUrl, sessionId, cancellationToken);

            var cookieHeader = BuildCookieHeader(flareSolverrResponse.Cookies);
            var userAgent = flareSolverrResponse.UserAgent;

            foreach (var band in uniqueBands)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var blobPath = $"{BandImagesFolder}/{band.MetalArchivesId}.jpg";

                    if (await _fileStorageService.FileExistsAsync(blobPath, cancellationToken))
                    {
                        _logger.LogInformation("Photo already exists for band '{BandName}' (MA ID: {MaId}).", band.BandName, band.MetalArchivesId);
                        _progressTracker.ItemProcessed(runId, band.BandName, "photoSkipped");

                        await PublishEventAsync(band, blobPath, cancellationToken);
                        await DelayBetweenRequests(settings, cancellationToken);
                        continue;
                    }

                    var bandPageUrl = $"{settings.MetalArchivesBaseUrl}/bands/_/{band.MetalArchivesId}";
                    var bandPageHtml = await _flareSolverrClient.GetPageContentAsync(bandPageUrl, sessionId, cancellationToken);
                    var photoUrl = ExtractPhotoUrl(bandPageHtml);

                    if (photoUrl == null)
                    {
                        _logger.LogWarning("No photo URL found on page for band '{BandName}' (MA ID: {MaId}).", band.BandName, band.MetalArchivesId);
                        _progressTracker.ItemFailed(runId, band.BandName, "No photo on band page", "photoFailed");
                        await DelayBetweenRequests(settings, cancellationToken);
                        continue;
                    }

                    var imageBytes = await DownloadImageAsync(photoUrl, cookieHeader, userAgent, cancellationToken);

                    if (imageBytes == null || !IsValidImage(imageBytes))
                    {
                        _logger.LogWarning("No valid photo found for band '{BandName}' (MA ID: {MaId}).", band.BandName, band.MetalArchivesId);
                        _progressTracker.ItemFailed(runId, band.BandName, "Invalid or missing photo", "photoFailed");
                        await DelayBetweenRequests(settings, cancellationToken);
                        continue;
                    }

                    using var imageStream = new MemoryStream(imageBytes);
                    await _fileStorageService.UploadFileAsync(blobPath, imageStream, cancellationToken);

                    _logger.LogInformation("Uploaded photo for band '{BandName}' (MA ID: {MaId}) to {BlobPath}.", band.BandName, band.MetalArchivesId, blobPath);
                    _progressTracker.ItemProcessed(runId, band.BandName, "photoUploaded");

                    await PublishEventAsync(band, blobPath, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (FlurlHttpException httpException) when (httpException.StatusCode == 404)
                {
                    _logger.LogWarning("Photo not found (404) for band '{BandName}' (MA ID: {MaId}).", band.BandName, band.MetalArchivesId);
                    _progressTracker.ItemFailed(runId, band.BandName, "Photo not found (404)", "photoFailed");
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Failed to sync photo for band '{BandName}' (MA ID: {MaId}).", band.BandName, band.MetalArchivesId);
                    _progressTracker.ItemFailed(runId, band.BandName, exception.Message, "photoFailed");
                }

                await DelayBetweenRequests(settings, cancellationToken);
            }

            _progressTracker.CompleteRun(runId);
            _logger.LogInformation("Band photo sync completed.");
        }
        catch (Exception exception)
        {
            _progressTracker.FailRun(runId, exception.Message);
            throw;
        }
        finally
        {
            await _flareSolverrClient.DestroySessionAsync(sessionId, cancellationToken);
        }
    }

    private async Task PublishEventAsync(
        Admin.Dtos.BandPhotoMetadataDto band,
        string blobPath,
        CancellationToken cancellationToken)
    {
        var syncedEvent = new BandPhotoSyncedEvent
        {
            BandName = band.BandName,
            PhotoBlobPath = blobPath,
            Genre = band.Genre,
        };

        await _topicProducer.Produce(syncedEvent, cancellationToken);
    }

    private async Task<byte[]?> DownloadImageAsync(
        string imageUrl,
        string? cookieHeader,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var effectiveUserAgent = userAgent ?? _userAgentProvider.GetRandomUserAgent();

        var request = imageUrl
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithHeader("User-Agent", effectiveUserAgent)
            .WithHeader("Accept", "image/webp,image/apng,image/*,*/*;q=0.8")
            .WithHeader("Accept-Encoding", "gzip, deflate, br");

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request = request.WithHeader("Cookie", cookieHeader);
        }

        return await request.GetBytesAsync(cancellationToken: cancellationToken);
    }

    private async Task DelayBetweenRequests(BandReferenceSettings settings, CancellationToken cancellationToken)
    {
        var delayMs = _random.Next(settings.MinRequestDelayMs, settings.MaxRequestDelayMs);
        await Task.Delay(delayMs, cancellationToken);
    }

    private static string? ExtractPhotoUrl(string bandPageHtml)
    {
        var document = new HtmlDocument();
        document.LoadHtml(bandPageHtml);

        var photoLink = document.DocumentNode.SelectSingleNode("//a[@class='image' and contains(@href, '_photo')]");
        if (photoLink != null)
        {
            return photoLink.GetAttributeValue("href", null);
        }

        var photoImg = document.DocumentNode.SelectSingleNode("//img[contains(@src, '_photo')]");
        return photoImg?.GetAttributeValue("src", null);
    }

    private static string? BuildCookieHeader(List<FlareSolverrCookie> cookies)
    {
        if (cookies.Count == 0)
        {
            return null;
        }

        return string.Join("; ", cookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));
    }

    private static bool IsValidImage(byte[] imageBytes)
    {
        if (imageBytes.Length < 100)
        {
            return false;
        }

        try
        {
            using var stream = new MemoryStream(imageBytes);
            return stream.IsImage();
        }
        catch
        {
            return false;
        }
    }
}
