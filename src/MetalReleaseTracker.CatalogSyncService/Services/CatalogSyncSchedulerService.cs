using MetalReleaseTracker.CatalogSyncService.Data;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace MetalReleaseTracker.CatalogSyncService.Services
{
    public class CatalogSyncSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CatalogSyncSchedulerService> _logger;

        public CatalogSyncSchedulerService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CatalogSyncSchedulerService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var cronTickerManager = scope.ServiceProvider.GetRequiredService<ICronTickerManager<CustomCronTicker>>();
            var tickerQDbContext = scope.ServiceProvider.GetRequiredService<CatalogSyncTickerQDbContext>();

            await RegisterAlbumProcessingJob(cronTickerManager, tickerQDbContext, cancellationToken);
            await RegisterAlbumProcessedPublisherJob(cronTickerManager, tickerQDbContext, cancellationToken);
        }

        private async Task RegisterAlbumProcessingJob(
            ICronTickerManager<CustomCronTicker> cronTickerManager,
            CatalogSyncTickerQDbContext tickerQDbContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var functionName = "AlbumProcessingJob";

                var existingJob = tickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName);

                if (existingJob != null)
                {
                    _logger.LogDebug("Skipping job {FunctionName} - already registered", functionName);
                    return;
                }

                await cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Expression = "0 0 */1 * * *",
                        Description = "Album processing job - runs every 4 hours",
                        Retries = 3,
                        RetryIntervals = [300, 900, 1800]
                    },
                    cancellationToken);

                _logger.LogInformation("Scheduled album processing job");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to schedule album processing job");
            }
        }

        private async Task RegisterAlbumProcessedPublisherJob(
            ICronTickerManager<CustomCronTicker> cronTickerManager,
            CatalogSyncTickerQDbContext tickerQDbContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var functionName = "AlbumProcessedPublisherJob";

                var existingJob = tickerQDbContext.Set<CustomCronTicker>()
                    .FirstOrDefault(job => job.Function == functionName);

                if (existingJob != null)
                {
                    _logger.LogDebug("Skipping job {FunctionName} - already registered", functionName);
                    return;
                }

                await cronTickerManager.AddAsync(
                    new CustomCronTicker
                    {
                        Function = functionName,
                        Expression = "0 0 */1 * * *",
                        Description = "Album processed publisher job - runs every hour",
                        Retries = 3,
                        RetryIntervals = [300, 900, 1800]
                    },
                    cancellationToken);

                _logger.LogInformation("Scheduled album processed publisher job");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to schedule album processed publisher job");
            }
        }
    }
}
