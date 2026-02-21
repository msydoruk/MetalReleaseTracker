using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

    private readonly ParserServiceDbContext _context;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeApiSettings _claudeSettings;
    private readonly ILogger<AiVerificationService> _logger;

    public AiVerificationService(
        ParserServiceDbContext context,
        ICatalogueIndexRepository catalogueIndexRepository,
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeApiSettings> claudeSettings,
        ILogger<AiVerificationService> logger)
    {
        _context = context;
        _catalogueIndexRepository = catalogueIndexRepository;
        _httpClientFactory = httpClientFactory;
        _claudeSettings = claudeSettings.Value;
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
        var relevantEntries = distributorCode.HasValue
            ? await _catalogueIndexRepository.GetByStatusAsync(distributorCode.Value, CatalogueIndexStatus.Relevant, cancellationToken)
            : await _catalogueIndexRepository.GetByStatusAsync(CatalogueIndexStatus.Relevant, cancellationToken);

        var existingCatalogueIds = await _context.AiVerifications
            .Where(verification => verification.AdminDecision == null)
            .Select(verification => verification.CatalogueIndexId)
            .ToHashSetAsync(cancellationToken);

        var toVerify = relevantEntries
            .Where(entry => !existingCatalogueIds.Contains(entry.Id))
            .ToList();

        _logger.LogInformation("AI Verification: {Count} entries to verify out of {Total} relevant.", toVerify.Count, relevantEntries.Count);

        var semaphore = new SemaphoreSlim(_claudeSettings.MaxConcurrentRequests);
        var verificationCount = 0;

        var tasks = toVerify.Select(async entry =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await CallClaudeApiAsync(entry.BandName, entry.AlbumTitle, cancellationToken);
                if (result != null)
                {
                    var entity = new AiVerificationEntity
                    {
                        Id = Guid.NewGuid(),
                        CatalogueIndexId = entry.Id,
                        BandName = entry.BandName,
                        AlbumTitle = entry.AlbumTitle,
                        IsUkrainian = result.IsUkrainian,
                        ConfidenceScore = result.Confidence,
                        AiAnalysis = result.Analysis,
                        CreatedAt = DateTime.UtcNow,
                    };

                    lock (_context)
                    {
                        _context.AiVerifications.Add(entity);
                    }

                    Interlocked.Increment(ref verificationCount);
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "AI Verification failed for band '{BandName}' - album '{AlbumTitle}'.", entry.BandName, entry.AlbumTitle);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI Verification completed. Created {Count} verifications.", verificationCount);
        return verificationCount;
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
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ClaudeVerificationResult?> CallClaudeApiAsync(
        string bandName,
        string albumTitle,
        CancellationToken cancellationToken)
    {
        var prompt = $$"""
            You are a metal music expert. Determine if the following band originates from Ukraine.

            Band: {{bandName}}
            Album: {{albumTitle}}

            Consider: the band's country of origin (not individual members' nationality),
            official sources like Metal Archives, Discogs, or band websites.

            Respond ONLY with JSON:
            {"isUkrainian": true/false, "confidence": 0.0-1.0, "analysis": "brief reasoning"}
            """;

        var requestBody = new
        {
            model = _claudeSettings.Model,
            max_tokens = _claudeSettings.MaxTokens,
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
        request.Headers.Add("x-api-key", _claudeSettings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

        var textContent = apiResponse
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(textContent))
        {
            return null;
        }

        var jsonStart = textContent.IndexOf('{');
        var jsonEnd = textContent.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd < 0)
        {
            return null;
        }

        var jsonString = textContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
        return JsonSerializer.Deserialize<ClaudeVerificationResult>(jsonString, JsonOptions);
    }

    private sealed class ClaudeVerificationResult
    {
        public bool IsUkrainian { get; set; }

        public double Confidence { get; set; }

        public string Analysis { get; set; } = string.Empty;
    }
}
