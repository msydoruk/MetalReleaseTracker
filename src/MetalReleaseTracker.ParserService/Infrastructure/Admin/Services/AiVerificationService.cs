using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;

public class AiVerificationService : IAiVerificationService
{
    private static readonly Dictionary<AiVerificationSortField, Expression<Func<AiVerificationDto, object>>> SortExpressions = new()
    {
        [AiVerificationSortField.BandName] = dto => dto.BandName,
        [AiVerificationSortField.ConfidenceScore] = dto => dto.ConfidenceScore!,
        [AiVerificationSortField.VerifiedAt] = dto => dto.VerifiedAt!,
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<int> FatalStatusCodes = [401, 402, 403, 429];

    private readonly ParserServiceDbContext _context;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<AiVerificationService> _logger;

    public AiVerificationService(
        ParserServiceDbContext context,
        ICatalogueIndexRepository catalogueIndexRepository,
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILogger<AiVerificationService> logger)
    {
        _context = context;
        _catalogueIndexRepository = catalogueIndexRepository;
        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<PagedResultDto<AiVerificationDto>> GetVerificationsAsync(
        AiVerificationFilterDto filter,
        CancellationToken cancellationToken)
    {
        var pendingVerifications = _context.AiVerifications
            .Where(verification => verification.AdminDecision == null);

        var query = (from catalogueEntry in _context.CatalogueIndex.AsNoTracking()
                where catalogueEntry.Status == CatalogueIndexStatus.Relevant
                join verification in pendingVerifications
                    on catalogueEntry.Id equals verification.CatalogueIndexId into verifications
                from verification in verifications.DefaultIfEmpty()
                join discography in _context.BandDiscography.AsNoTracking()
                    on verification.MatchedBandDiscographyId equals discography.Id into discographies
                from discography in discographies.DefaultIfEmpty()
                select new AiVerificationDto
                {
                    Id = catalogueEntry.Id,
                    BandName = catalogueEntry.BandName,
                    AlbumTitle = catalogueEntry.AlbumTitle ?? string.Empty,
                    DistributorCode = catalogueEntry.DistributorCode,
                    VerificationId = verification != null ? verification.Id : (Guid?)null,
                    IsUkrainian = verification != null ? verification.IsUkrainian : (bool?)null,
                    ConfidenceScore = verification != null ? verification.ConfidenceScore : (double?)null,
                    AiAnalysis = verification != null ? verification.AiAnalysis : null,
                    AdminDecision = verification != null ? verification.AdminDecision : null,
                    AdminDecisionDate = verification != null ? verification.AdminDecisionDate : null,
                    MatchedBandDiscographyId = verification != null ? verification.MatchedBandDiscographyId : null,
                    MatchedAlbumTitle = discography != null ? discography.AlbumTitle : null,
                    MatchedAlbumType = discography != null ? discography.AlbumType : null,
                    MatchedAlbumYear = discography != null ? discography.Year : null,
                    VerifiedAt = verification != null ? verification.CreatedAt : (DateTime?)null,
                })
            .WhereIf(filter.DistributorCode.HasValue, dto => dto.DistributorCode == filter.DistributorCode!.Value)
            .WhereIf(filter.IsUkrainian.HasValue, dto => dto.IsUkrainian == filter.IsUkrainian!.Value)
            .WhereIf(filter.VerifiedOnly == true, dto => dto.VerificationId != null)
            .WhereIf(filter.VerifiedOnly == false, dto => dto.VerificationId == null);

        var sortField = filter.SortBy ?? AiVerificationSortField.BandName;
        if (SortExpressions.TryGetValue(sortField, out var expression))
        {
            query = (filter.SortAscending ?? true)
                ? query.OrderBy(expression)
                : query.OrderByDescending(expression);
        }

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<int> RunVerificationAsync(
        DistributorCode? distributorCode,
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<VerificationProgressEvent>();
        var count = 0;

        var writeTask = RunVerificationStreamAsync(distributorCode, channel.Writer, cancellationToken);

        await foreach (var progressEvent in channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (progressEvent.Type == "completed")
            {
                count = progressEvent.Processed - progressEvent.Failed;
            }
        }

        await writeTask;
        return count;
    }

    public async Task RunVerificationStreamAsync(
        DistributorCode? distributorCode,
        ChannelWriter<VerificationProgressEvent> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            await RunVerificationCoreAsync(distributorCode, writer, cancellationToken);
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task RunVerificationCoreAsync(
        DistributorCode? distributorCode,
        ChannelWriter<VerificationProgressEvent> writer,
        CancellationToken cancellationToken)
    {
        var agent = await _settingsService.GetActiveAiAgentAsync(cancellationToken);
        if (agent == null)
        {
            _logger.LogWarning("AI Verification: No active AI agent configured. Skipping verification.");
            await writer.WriteAsync(new VerificationProgressEvent("error", 0, 0, 0, null, "No active AI agent configured. Add one in Settings."), cancellationToken);
            return;
        }

        var relevantEntries = distributorCode.HasValue
            ? await _catalogueIndexRepository.GetByStatusAsync(distributorCode.Value, CatalogueIndexStatus.Relevant, cancellationToken)
            : await _catalogueIndexRepository.GetByStatusAsync(CatalogueIndexStatus.Relevant, cancellationToken);

        var toVerify = relevantEntries.ToList();

        var existingPendingVerifications = await _context.AiVerifications
            .Where(verification => verification.AdminDecision == null)
            .Where(verification => toVerify.Select(entry => entry.Id).Contains(verification.CatalogueIndexId))
            .ToListAsync(cancellationToken);

        if (existingPendingVerifications.Count > 0)
        {
            _context.AiVerifications.RemoveRange(existingPendingVerifications);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("AI Verification: Removed {Count} existing pending verifications for re-run.", existingPendingVerifications.Count);
        }

        _logger.LogInformation("AI Verification: {Count} entries to verify out of {Total} relevant.", toVerify.Count, relevantEntries.Count);

        await writer.WriteAsync(new VerificationProgressEvent("started", 0, toVerify.Count, 0, null, null), cancellationToken);

        if (toVerify.Count == 0)
        {
            await writer.WriteAsync(new VerificationProgressEvent("completed", 0, 0, 0, null, null), cancellationToken);
            return;
        }

        var bandReferenceIds = toVerify
            .Where(entry => entry.BandReferenceId.HasValue)
            .Select(entry => entry.BandReferenceId!.Value)
            .Distinct()
            .ToList();

        var discographyByBandReference = await _context.BandDiscography
            .Where(discography => bandReferenceIds.Contains(discography.BandReferenceId))
            .GroupBy(discography => discography.BandReferenceId)
            .ToDictionaryAsync(
                group => group.Key,
                group => (
                    FormattedText: string.Join("\n", group.Select(discography =>
                        $"- [{discography.Id}] {discography.AlbumTitle} ({discography.AlbumType}, {discography.Year})")),
                    ValidIds: new HashSet<Guid>(group.Select(discography => discography.Id))),
                cancellationToken);

        var semaphore = new SemaphoreSlim(agent.MaxConcurrentRequests);
        var processed = 0;
        var failed = 0;
        var fatalError = false;

        var tasks = toVerify.Select(async entry =>
        {
            await semaphore.WaitAsync(cancellationToken);

            if (Volatile.Read(ref fatalError))
            {
                semaphore.Release();
                return;
            }

            try
            {
                var discography = string.Empty;
                HashSet<Guid>? validIds = null;
                if (entry.BandReferenceId.HasValue && discographyByBandReference.TryGetValue(entry.BandReferenceId.Value, out var disco))
                {
                    discography = disco.FormattedText;
                    validIds = disco.ValidIds;
                }

                var (result, error) = await CallClaudeApiSafeAsync(agent, entry.BandName, entry.AlbumTitle, discography, cancellationToken);

                if (error != null)
                {
                    if (error.IsFatal)
                    {
                        Volatile.Write(ref fatalError, true);
                        Interlocked.Increment(ref failed);
                        await writer.WriteAsync(new VerificationProgressEvent("error", Volatile.Read(ref processed), toVerify.Count, Volatile.Read(ref failed), $"{entry.BandName} - {entry.AlbumTitle}", error.Message), cancellationToken);
                        return;
                    }

                    _logger.LogWarning("AI Verification failed for band '{BandName}' - album '{AlbumTitle}': {Error}", entry.BandName, entry.AlbumTitle, error.Message);
                    Interlocked.Increment(ref failed);
                }
                else if (result != null)
                {
                    var matchedId = ParseGuidOrNull(result.MatchedAlbumId);
                    if (matchedId.HasValue && validIds != null && !validIds.Contains(matchedId.Value))
                    {
                        _logger.LogWarning("AI returned invalid discography ID {Id} for band '{BandName}' - album '{AlbumTitle}'. Ignoring.", matchedId.Value, entry.BandName, entry.AlbumTitle);
                        matchedId = null;
                    }

                    var entity = new AiVerificationEntity
                    {
                        Id = Guid.NewGuid(),
                        CatalogueIndexId = entry.Id,
                        BandName = entry.BandName,
                        AlbumTitle = entry.AlbumTitle,
                        IsUkrainian = result.IsUkrainian,
                        ConfidenceScore = result.Confidence,
                        AiAnalysis = result.Analysis,
                        MatchedBandDiscographyId = matchedId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    lock (_context)
                    {
                        _context.AiVerifications.Add(entity);
                    }
                }

                var currentProcessed = Interlocked.Increment(ref processed);
                await writer.WriteAsync(new VerificationProgressEvent("progress", currentProcessed, toVerify.Count, Volatile.Read(ref failed), $"{entry.BandName} - {entry.AlbumTitle}", null), cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "AI Verification failed for band '{BandName}' - album '{AlbumTitle}'.", entry.BandName, entry.AlbumTitle);
                Interlocked.Increment(ref failed);
                Interlocked.Increment(ref processed);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        await _context.SaveChangesAsync(cancellationToken);

        var totalProcessed = Volatile.Read(ref processed);
        var totalFailed = Volatile.Read(ref failed);
        _logger.LogInformation("AI Verification completed. Created {Count} verifications ({Failed} failed).", totalProcessed - totalFailed, totalFailed);

        if (!fatalError)
        {
            await writer.WriteAsync(new VerificationProgressEvent("completed", totalProcessed, toVerify.Count, totalFailed, null, null), cancellationToken);
        }
    }

    public async Task SetDecisionAsync(
        Guid verificationId,
        AiVerificationDecision decision,
        CancellationToken cancellationToken)
    {
        var verification = await _context.AiVerifications.FirstOrDefaultAsync(verification => verification.Id == verificationId, cancellationToken);
        if (verification == null)
        {
            return;
        }

        verification.AdminDecision = decision;
        verification.AdminDecisionDate = DateTime.UtcNow;

        if (decision == AiVerificationDecision.Rejected)
        {
            await _catalogueIndexRepository.UpdateStatusAsync(verification.CatalogueIndexId, CatalogueIndexStatus.NotRelevant, cancellationToken);
        }
        else if (decision == AiVerificationDecision.Confirmed)
        {
            await _catalogueIndexRepository.UpdateStatusAsync(verification.CatalogueIndexId, CatalogueIndexStatus.AiVerified, cancellationToken);

            if (verification.MatchedBandDiscographyId.HasValue)
            {
                var catalogueEntry = await _context.CatalogueIndex.FindAsync([verification.CatalogueIndexId], cancellationToken);
                if (catalogueEntry != null)
                {
                    catalogueEntry.BandDiscographyId = verification.MatchedBandDiscographyId;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetBatchDecisionAsync(
        List<Guid> ids,
        AiVerificationDecision decision,
        CancellationToken cancellationToken)
    {
        var verifications = await _context.AiVerifications
            .Where(verification => ids.Contains(verification.Id))
            .ToListAsync(cancellationToken);

        var rejectedCatalogueIds = new List<Guid>();
        var confirmedCatalogueIds = new List<Guid>();

        foreach (var verification in verifications)
        {
            verification.AdminDecision = decision;
            verification.AdminDecisionDate = DateTime.UtcNow;

            if (decision == AiVerificationDecision.Rejected)
            {
                rejectedCatalogueIds.Add(verification.CatalogueIndexId);
            }
            else if (decision == AiVerificationDecision.Confirmed)
            {
                confirmedCatalogueIds.Add(verification.CatalogueIndexId);
            }
        }

        if (rejectedCatalogueIds.Count > 0)
        {
            await _catalogueIndexRepository.UpdateStatusBatchAsync(rejectedCatalogueIds, CatalogueIndexStatus.NotRelevant, cancellationToken);
        }

        if (confirmedCatalogueIds.Count > 0)
        {
            await _catalogueIndexRepository.UpdateStatusBatchAsync(confirmedCatalogueIds, CatalogueIndexStatus.AiVerified, cancellationToken);

            var matchedDiscographyIds = verifications
                .Where(verification => verification.AdminDecision == AiVerificationDecision.Confirmed && verification.MatchedBandDiscographyId.HasValue)
                .ToDictionary(verification => verification.CatalogueIndexId, verification => verification.MatchedBandDiscographyId!.Value);

            if (matchedDiscographyIds.Count > 0)
            {
                var catalogueEntries = await _context.CatalogueIndex
                    .Where(entry => matchedDiscographyIds.Keys.Contains(entry.Id))
                    .ToListAsync(cancellationToken);

                foreach (var entry in catalogueEntries)
                {
                    entry.BandDiscographyId = matchedDiscographyIds[entry.Id];
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SetBulkDecisionByFilterAsync(
        DistributorCode? distributorCode,
        bool? isUkrainian,
        AiVerificationDecision decision,
        CancellationToken cancellationToken)
    {
        var query = _context.AiVerifications
            .Where(verification => verification.AdminDecision == null);

        if (distributorCode.HasValue)
        {
            var catalogueIds = await _context.CatalogueIndex
                .Where(entry => entry.DistributorCode == distributorCode.Value)
                .Select(entry => entry.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(verification => catalogueIds.Contains(verification.CatalogueIndexId));
        }

        if (isUkrainian.HasValue)
        {
            query = query.Where(verification => verification.IsUkrainian == isUkrainian.Value);
        }

        var verifications = await query.ToListAsync(cancellationToken);
        if (verifications.Count == 0)
        {
            return 0;
        }

        var rejectedCatalogueIds = new List<Guid>();
        var confirmedCatalogueIds = new List<Guid>();
        var matchedDiscographyIds = new Dictionary<Guid, Guid>();

        foreach (var verification in verifications)
        {
            verification.AdminDecision = decision;
            verification.AdminDecisionDate = DateTime.UtcNow;

            if (decision == AiVerificationDecision.Rejected)
            {
                rejectedCatalogueIds.Add(verification.CatalogueIndexId);
            }
            else if (decision == AiVerificationDecision.Confirmed)
            {
                confirmedCatalogueIds.Add(verification.CatalogueIndexId);

                if (verification.MatchedBandDiscographyId.HasValue)
                {
                    matchedDiscographyIds[verification.CatalogueIndexId] = verification.MatchedBandDiscographyId.Value;
                }
            }
        }

        if (rejectedCatalogueIds.Count > 0)
        {
            await _catalogueIndexRepository.UpdateStatusBatchAsync(rejectedCatalogueIds, CatalogueIndexStatus.NotRelevant, cancellationToken);
        }

        if (confirmedCatalogueIds.Count > 0)
        {
            await _catalogueIndexRepository.UpdateStatusBatchAsync(confirmedCatalogueIds, CatalogueIndexStatus.AiVerified, cancellationToken);

            if (matchedDiscographyIds.Count > 0)
            {
                var catalogueEntries = await _context.CatalogueIndex
                    .Where(entry => matchedDiscographyIds.Keys.Contains(entry.Id))
                    .ToListAsync(cancellationToken);

                foreach (var entry in catalogueEntries)
                {
                    entry.BandDiscographyId = matchedDiscographyIds[entry.Id];
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk decision applied: {Decision} to {Count} verifications.", decision, verifications.Count);
        return verifications.Count;
    }

    private async Task<(ClaudeVerificationResult? Result, ApiCallError? Error)> CallClaudeApiSafeAsync(
        AiAgentEntity agent,
        string bandName,
        string albumTitle,
        string discography,
        CancellationToken cancellationToken)
    {
        try
        {
            var prompt = agent.SystemPrompt
                .Replace("{{bandName}}", bandName)
                .Replace("{{albumTitle}}", albumTitle)
                .Replace("{{discography}}", discography);

            var requestBody = new
            {
                model = agent.Model,
                max_tokens = agent.MaxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt },
                },
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("x-api-key", agent.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusCode = (int)response.StatusCode;
                var errorMessage = ExtractApiErrorMessage(errorBody, statusCode);
                var isFatal = FatalStatusCodes.Contains(statusCode);
                return (null, new ApiCallError(errorMessage, isFatal));
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var textContent = apiResponse
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(textContent))
            {
                return (null, new ApiCallError("Empty response from Claude API", false));
            }

            var jsonStart = textContent.IndexOf('{');
            var jsonEnd = textContent.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd < 0)
            {
                return (null, new ApiCallError("Could not parse JSON from Claude response", false));
            }

            var jsonString = textContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var result = JsonSerializer.Deserialize<ClaudeVerificationResult>(jsonString, JsonOptions);
            return (result, null);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return (null, new ApiCallError(exception.Message, false));
        }
    }

    private static string ExtractApiErrorMessage(string errorBody, int statusCode)
    {
        try
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
            if (errorJson.TryGetProperty("error", out var errorObj) &&
                errorObj.TryGetProperty("message", out var message))
            {
                return $"Claude API {statusCode}: {message.GetString()}";
            }
        }
        catch
        {
            // Ignore JSON parse errors for error body
        }

        return statusCode switch
        {
            401 => "Claude API 401: Invalid API key",
            402 => "Claude API 402: Insufficient credits",
            403 => "Claude API 403: Access denied",
            429 => "Claude API 429: Rate limit exceeded",
            _ => $"Claude API {statusCode}: {errorBody[..Math.Min(errorBody.Length, 200)]}",
        };
    }

    private sealed record ApiCallError(string Message, bool IsFatal);

    private sealed class ClaudeVerificationResult
    {
        public bool IsUkrainian { get; set; }

        public double Confidence { get; set; }

        public string Analysis { get; set; } = string.Empty;

        public string? MatchedAlbumId { get; set; }
    }

    private static Guid? ParseGuidOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "null")
        {
            return null;
        }

        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
