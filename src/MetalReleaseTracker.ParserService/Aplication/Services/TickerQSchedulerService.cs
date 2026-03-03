using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using TickerQ;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;

namespace MetalReleaseTracker.ParserService.Aplication.Services;

public class TickerQSchedulerService : BackgroundService
{
    private readonly ICronTickerManager<CustomCronTicker> _cronTickerManager;
    private readonly ParserServiceTickerQDbContext _parserServiceTickerQDbContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TickerQSchedulerService> _logger;

    public TickerQSchedulerService(
        ICronTickerManager<CustomCronTicker> cronTickerManager,
        ParserServiceTickerQDbContext parserServiceTickerQDbContext,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TickerQSchedulerService> logger)
    {
        _cronTickerManager = cronTickerManager;
        _parserServiceTickerQDbContext = parserServiceTickerQDbContext;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var enabledSources = await settingsService.GetEnabledParsingSourcesAsync(cancellationToken);

        var parserDataSources = enabledSources.Select(source => new ParserDataSource
        {
            DistributorCode = source.DistributorCode,
            Name = source.Name,
            ParsingUrl = source.ParsingUrl,
        }).ToList();

        await RegisterBandReferenceSyncJob(cancellationToken);
        await RegisterCatalogueIndexJobs(parserDataSources, cancellationToken);
        await RegisterAlbumDetailParsingJobs(parserDataSources, cancellationToken);
        await RegisterAlbumPublisherJob(cancellationToken);
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
                    Description = "Weekly Ukrainian bands sync from Metal Archives"
                },
                cancellationToken);

            _logger.LogInformation("Scheduled band reference sync job (weekly).");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to schedule band reference sync job.");
        }
    }

    private async Task RegisterCatalogueIndexJobs(List<ParserDataSource> parserDataSources, CancellationToken cancellationToken = default)
    {
        foreach (var parserDataSource in parserDataSources)
        {
            try
            {
                var functionName = "CatalogueIndexJob";
                var request = TickerHelper.CreateTickerRequest(parserDataSource);
                var cronExpression = BuildWeeklyCron(parserDataSource.DistributorCode, 2);
                var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName && job.Request == request);

                if (existingJob != null)
                {
                    if (existingJob.Expression != cronExpression)
                    {
                        existingJob.Expression = cronExpression;
                        existingJob.Description = $"Weekly catalogue index for {parserDataSource.Name}";
                        await _parserServiceTickerQDbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation(
                            "Updated catalogue index job schedule for distributor: {DistributorCode} to {CronExpression}.",
                            parserDataSource.DistributorCode,
                            cronExpression);
                    }

                    continue;
                }

                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Request = request,
                        Expression = cronExpression,
                        Description = $"Weekly catalogue index for {parserDataSource.Name}"
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled catalogue index job for distributor: {DistributorCode} ({CronExpression}).",
                    parserDataSource.DistributorCode,
                    cronExpression);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Failed to schedule catalogue index job for distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
            }
        }
    }

    private async Task RegisterAlbumDetailParsingJobs(List<ParserDataSource> parserDataSources, CancellationToken cancellationToken = default)
    {
        foreach (var parserDataSource in parserDataSources)
        {
            try
            {
                var functionName = "AlbumDetailParsingJob";
                var request = TickerHelper.CreateTickerRequest(parserDataSource);
                var cronExpression = BuildWeeklyCron(parserDataSource.DistributorCode, 8);
                var existingJob = _parserServiceTickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName && job.Request == request);

                if (existingJob != null)
                {
                    if (existingJob.Expression != cronExpression)
                    {
                        existingJob.Expression = cronExpression;
                        existingJob.Description = $"Weekly album detail parsing for {parserDataSource.Name}";
                        await _parserServiceTickerQDbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation(
                            "Updated album detail parsing job schedule for distributor: {DistributorCode} to {CronExpression}.",
                            parserDataSource.DistributorCode,
                            cronExpression);
                    }

                    continue;
                }

                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Request = request,
                        Expression = cronExpression,
                        Description = $"Weekly album detail parsing for {parserDataSource.Name}"
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled album detail parsing job for distributor: {DistributorCode} ({CronExpression}).",
                    parserDataSource.DistributorCode,
                    cronExpression);
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

    private async Task RegisterAlbumPublisherJob(CancellationToken cancellationToken = default)
    {
        try
        {
            var functionName = "AlbumPublisherJob";
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
                    Expression = "0 0 */1 * * *",
                    Description = "Album publishing job every 1 hour"
                },
                cancellationToken);

            _logger.LogInformation("Scheduled album publisher job.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to schedule album publisher job.");
        }
    }

    private static string BuildWeeklyCron(DistributorCode code, int hour)
    {
        var dayOfWeek = (int)code % 7;
        return $"0 0 {hour} * * {dayOfWeek}";
    }
}
