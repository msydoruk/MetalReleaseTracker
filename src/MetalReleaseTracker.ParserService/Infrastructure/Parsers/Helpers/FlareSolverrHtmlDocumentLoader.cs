using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public class FlareSolverrHtmlDocumentLoader : IHtmlDocumentLoader, IAsyncDisposable
{
    private readonly IFlareSolverrClient _flareSolverrClient;
    private readonly ILogger<FlareSolverrHtmlDocumentLoader> _logger;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);
    private string? _sessionId;
    private bool _disposed;

    public FlareSolverrHtmlDocumentLoader(
        IFlareSolverrClient flareSolverrClient,
        ILogger<FlareSolverrHtmlDocumentLoader> logger)
    {
        _flareSolverrClient = flareSolverrClient;
        _logger = logger;
    }

    public async Task<HtmlDocument> LoadHtmlDocumentAsync(string url, CancellationToken cancellationToken)
    {
        var sessionId = await EnsureSessionAsync(cancellationToken);
        var pageContent = await _flareSolverrClient.GetPageContentAsync(url, sessionId, cancellationToken);

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
