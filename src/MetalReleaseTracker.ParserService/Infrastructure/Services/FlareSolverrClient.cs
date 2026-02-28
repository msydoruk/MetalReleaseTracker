using System.Text;
using System.Text.Json;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Services;

public interface IFlareSolverrClient
{
    Task<string> CreateSessionAsync(CancellationToken cancellationToken);

    Task<string> GetPageContentAsync(string url, string sessionId, CancellationToken cancellationToken);

    Task DestroySessionAsync(string sessionId, CancellationToken cancellationToken);
}

public class FlareSolverrClient : IFlareSolverrClient
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<FlareSolverrClient> _logger;
    private FlareSolverrSettings? _cachedSettings;

    public FlareSolverrClient(
        HttpClient httpClient,
        ISettingsService settingsService,
        ILogger<FlareSolverrClient> logger)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(CancellationToken cancellationToken)
    {
        _cachedSettings ??= await _settingsService.GetFlareSolverrSettingsAsync(cancellationToken);
        var response = await PostCommandAsync(_cachedSettings, new { cmd = "sessions.create" }, cancellationToken);
        var session = response.RootElement.GetProperty("session").GetString()
            ?? throw new InvalidOperationException("FlareSolverr returned empty session ID.");

        _logger.LogInformation("Created FlareSolverr session: {SessionId}.", session);
        return session;
    }

    public async Task<string> GetPageContentAsync(string url, string sessionId, CancellationToken cancellationToken)
    {
        _cachedSettings ??= await _settingsService.GetFlareSolverrSettingsAsync(cancellationToken);

        var payload = new
        {
            cmd = "request.get",
            url,
            session = sessionId,
            maxTimeout = _cachedSettings.MaxTimeoutMs
        };

        var response = await PostCommandAsync(_cachedSettings, payload, cancellationToken);
        var status = response.RootElement.GetProperty("status").GetString();

        if (status != "ok")
        {
            var message = response.RootElement.GetProperty("message").GetString();
            throw new InvalidOperationException($"FlareSolverr error: {message}");
        }

        var solution = response.RootElement.GetProperty("solution");
        var httpStatus = solution.GetProperty("status").GetInt32();

        _logger.LogInformation("FlareSolverr fetched {Url} with HTTP status {Status}.", url, httpStatus);

        return solution.GetProperty("response").GetString()
            ?? throw new InvalidOperationException("FlareSolverr returned empty response body.");
    }

    public async Task DestroySessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        try
        {
            _cachedSettings ??= await _settingsService.GetFlareSolverrSettingsAsync(cancellationToken);
            await PostCommandAsync(_cachedSettings, new { cmd = "sessions.destroy", session = sessionId }, cancellationToken);
            _logger.LogInformation("Destroyed FlareSolverr session: {SessionId}.", sessionId);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to destroy FlareSolverr session: {SessionId}.", sessionId);
        }
    }

    private async Task<JsonDocument> PostCommandAsync(FlareSolverrSettings settings, object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{settings.BaseUrl}/v1", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(responseBody);
    }
}
