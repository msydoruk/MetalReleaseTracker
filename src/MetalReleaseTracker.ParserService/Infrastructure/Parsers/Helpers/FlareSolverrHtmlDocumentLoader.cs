using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public class FlareSolverrHtmlDocumentLoader : IHtmlDocumentLoader, IAsyncDisposable
{
    private const int DelayBetweenRequestsMs = 3000;
    private const int DelayBeforeRetryMs = 5000;

    private readonly IFlareSolverrClient _flareSolverrClient;
    private readonly ILogger<FlareSolverrHtmlDocumentLoader> _logger;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);
    private string? _sessionId;
    private bool _disposed;
    private bool _isFirstRequest = true;

    public FlareSolverrHtmlDocumentLoader(
        IFlareSolverrClient flareSolverrClient,
        ILogger<FlareSolverrHtmlDocumentLoader> logger)
    {
        _flareSolverrClient = flareSolverrClient;
        _logger = logger;
    }

    public async Task<HtmlDocument> LoadHtmlDocumentAsync(string url, CancellationToken cancellationToken)
    {
        if (!_isFirstRequest)
        {
            await Task.Delay(DelayBetweenRequestsMs, cancellationToken);
        }

        _isFirstRequest = false;

        var sessionId = await EnsureSessionAsync(cancellationToken);

        string pageContent;
        try
        {
            pageContent = await _flareSolverrClient.GetPageContentAsync(url, sessionId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "FlareSolverr request failed for {Url}. Waiting {Delay}ms, recreating session and retrying.", url, DelayBeforeRetryMs);
            await Task.Delay(DelayBeforeRetryMs, cancellationToken);
            sessionId = await RecreateSessionAsync(cancellationToken);
            pageContent = await _flareSolverrClient.GetPageContentAsync(url, sessionId, cancellationToken);
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(pageContent);

        return htmlDocument;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_sessionId != null)
        {
            _logger.LogInformation("Disposing FlareSolverr session: {SessionId}.", _sessionId);
            await _flareSolverrClient.DestroySessionAsync(_sessionId, CancellationToken.None);
            _sessionId = null;
        }

        _sessionLock.Dispose();
    }

    private async Task<string> RecreateSessionAsync(CancellationToken cancellationToken)
    {
        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            if (_sessionId != null)
            {
                _logger.LogInformation("Destroying stale FlareSolverr session: {SessionId}.", _sessionId);
                await _flareSolverrClient.DestroySessionAsync(_sessionId, cancellationToken);
                _sessionId = null;
            }

            _logger.LogInformation("Creating new FlareSolverr session after failure.");
            _sessionId = await _flareSolverrClient.CreateSessionAsync(cancellationToken);

            return _sessionId;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    private async Task<string> EnsureSessionAsync(CancellationToken cancellationToken)
    {
        if (_sessionId != null)
        {
            return _sessionId;
        }

        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            if (_sessionId != null)
            {
                return _sessionId;
            }

            _logger.LogInformation("Creating FlareSolverr session for HTML document loading.");
            _sessionId = await _flareSolverrClient.CreateSessionAsync(cancellationToken);

            return _sessionId;
        }
        finally
        {
            _sessionLock.Release();
        }
    }
}
