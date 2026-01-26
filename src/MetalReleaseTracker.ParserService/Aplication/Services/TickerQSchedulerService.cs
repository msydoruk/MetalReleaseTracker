using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using Microsoft.Extensions.Options;
using TickerQ;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;

namespace MetalReleaseTracker.ParserService.Aplication.Services;

public class TickerQSchedulerService : BackgroundService
{
    private readonly ICronTickerManager<CustomCronTicker> _cronTickerManager;
    private readonly List<ParserDataSource> _parserDataSources;
    private readonly ILogger<TickerQSchedulerService> _logger;

    public TickerQSchedulerService(
        ICronTickerManager<CustomCronTicker> cronTickerManager,
        IOptions<List<ParserDataSource>> parserDataSources,
        ILogger<TickerQSchedulerService> logger)
    {
        _cronTickerManager = cronTickerManager;
        _parserDataSources = parserDataSources.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        foreach (var parserDataSource in _parserDataSources)
        {
            try
            {
                await _cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = "AlbumParsingJob",
                        Request = TickerHelper.CreateTickerRequest(parserDataSource),
                        Expression = "0 0 0 * * *",
                        Description = $"Daily album parsing for {parserDataSource.Name}",
                        Retries = 0
                    },
                    cancellationToken);

                _logger.LogInformation(
                    "Scheduled album parsing job for distributor: {DistributorCode}",
                    parserDataSource.DistributorCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to schedule album parsing job for distributor: {DistributorCode}",
                    parserDataSource.DistributorCode);
            }
        }

        try
        {
            await _cronTickerManager.AddAsync(
                new CustomCronTicker
                {
                    Function = "AlbumParsedPublisherJob",
                    Expression = "0 0 0 * * *",
                    Description = "Daily album publishing job",
                    Retries = 10,
                    RetryIntervals = [60, 120, 300, 600, 900, 1800, 3600, 7200, 14400, 28800]
                },
                cancellationToken);

            _logger.LogInformation("Scheduled album parsed publisher job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule album parsed publisher job");
        }
    }
}
