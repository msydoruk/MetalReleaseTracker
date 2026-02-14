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
        await RegisterAlbumParsingJobs(cancellationToken);
        await RegisterParsedPublisherJob(cancellationToken);
    }

    private async Task RegisterAlbumParsingJobs(CancellationToken cancellationToken = default)
    {
        foreach (var parserDataSource in _parserDataSources)
        {
            try
            {
                var functionName = "AlbumParsingJob";
                var request = TickerHelper.CreateTickerRequest(parserDataSource);
                var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName && job.Request == request);

                if (existingJob != null)
                {
                    _logger.LogDebug($"Skipping job {functionName} with request {request}");
                    continue;
                }

                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Request = request,
                        Expression = "0 0 */24 * * *",
                        Description = $"Daily album parsing for {parserDataSource.Name}",
                        Retries = 3,
                        RetryIntervals = [300, 900, 1800]
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled album parsing job for distributor: {DistributorCode}",
                    parserDataSource.DistributorCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Failed to schedule album parsing job for distributor: {DistributorCode}",
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
                _logger.LogDebug($"Skipping job {functionName}");
                return;
            }

            await _cronTickerManager.AddAsync(
                new CustomCronTicker
                {
                    Function = functionName,
                    Expression = "0 0 */1 * * *",
                    Description = "Daily album publishing job",
                    Retries = 3,
                    RetryIntervals = [300, 900, 1800]
                },
                cancellationToken);

            _logger.LogInformation("Scheduled album parsed publisher job");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to schedule album parsed publisher job");
        }
    }
}
