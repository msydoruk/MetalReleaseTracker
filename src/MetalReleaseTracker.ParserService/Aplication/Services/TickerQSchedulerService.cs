using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using Microsoft.Extensions.Options;
using TickerQ;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;

namespace MetalReleaseTracker.ParserService.Aplication.Services;

public class TickerQSchedulerService : BackgroundService
{
    private readonly ICronTickerManager<CustomCronTicker> _cronTickerManager;
    private readonly ParserServiceTickerQDbContext _parserServiceTickerQDbContext;
    private readonly List<ParserDataSource> _parserDataSources;
    private readonly ILogger<TickerQSchedulerService> _logger;

    public TickerQSchedulerService(
        ICronTickerManager<CustomCronTicker> cronTickerManager,
        ParserServiceTickerQDbContext parserServiceTickerQDbContext,
        IOptions<List<ParserDataSource>> parserDataSources,
        ILogger<TickerQSchedulerService> logger)
    {
        _cronTickerManager = cronTickerManager;
        _parserServiceTickerQDbContext = parserServiceTickerQDbContext;
        _parserDataSources = parserDataSources.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await RegisterBandReferenceSyncJob(cancellationToken);
        await RegisterCatalogueIndexJobs(cancellationToken);
        await RegisterAlbumDetailParsingJobs(cancellationToken);
        await RegisterParsedPublisherJob(cancellationToken);
    }

    private async Task RegisterBandReferenceSyncJob(CancellationToken cancellationToken = default)
    {
        try
        {
            var functionName = "BandReferenceSyncJob";
            var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                .FirstOrDefault(job => job.Function == functionName);

            if (existingJob != null)
            {
                _logger.LogDebug("Skipping job {FunctionName}.", functionName);
                return;
            }

            await _cronTickerManager.AddAsync(
                new CustomCronTicker
                {
                    Function = functionName,
                    Expression = "0 0 0 * * 0",
                    Description = "Weekly Ukrainian bands sync from Metal Archives",
                    Retries = 3,
                    RetryIntervals = [300, 900, 1800]
                },
                cancellationToken);

            _logger.LogInformation("Scheduled band reference sync job (weekly).");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to schedule band reference sync job.");
        }
    }

    private async Task RegisterCatalogueIndexJobs(CancellationToken cancellationToken = default)
    {
        foreach (var parserDataSource in _parserDataSources)
        {
            try
            {
                var functionName = "CatalogueIndexJob";
                var request = TickerHelper.CreateTickerRequest(parserDataSource);
                var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName && job.Request == request);

                if (existingJob != null)
                {
                    _logger.LogDebug("Skipping job {FunctionName} with request {Request}.", functionName, request);
                    continue;
                }

                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Request = request,
                        Expression = "0 0 2 */3 * *",
                        Description = $"Catalogue index every 3 days for {parserDataSource.Name}",
                        Retries = 3,
                        RetryIntervals = [300, 900, 1800]
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled catalogue index job for distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Failed to schedule catalogue index job for distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
            }
        }
    }

    private async Task RegisterAlbumDetailParsingJobs(CancellationToken cancellationToken = default)
    {
        foreach (var parserDataSource in _parserDataSources)
        {
            try
            {
                var functionName = "AlbumDetailParsingJob";
                var request = TickerHelper.CreateTickerRequest(parserDataSource);
                var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName && job.Request == request);

                if (existingJob != null)
                {
                    _logger.LogDebug("Skipping job {FunctionName} with request {Request}.", functionName, request);
                    continue;
                }

                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Request = request,
                        Expression = "0 0 8 */3 * *",
                        Description = $"Album detail parsing every 3 days for {parserDataSource.Name}",
                        Retries = 3,
                        RetryIntervals = [300, 900, 1800]
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled album detail parsing job for distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to schedule album detail parsing job for distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
            }
        }
    }

    private async Task RegisterParsedPublisherJob(CancellationToken cancellationToken = default)
    {
        try
        {
            var functionName = "AlbumParsedPublisherJob";
            var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                .FirstOrDefault(job => job.Function == functionName);

            if (existingJob != null)
            {
                _logger.LogDebug("Skipping job {FunctionName}.", functionName);
                return;
            }

            await _cronTickerManager.AddAsync(
                new CustomCronTicker
                {
                    Function = functionName,
                    Expression = "0 0 */6 * * *",
                    Description = "Album publishing job every 6 hours",
                    Retries = 3,
                    RetryIntervals = [300, 900, 1800]
                },
                cancellationToken);

            _logger.LogInformation("Scheduled album parsed publisher job.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to schedule album parsed publisher job.");
        }
    }
}
