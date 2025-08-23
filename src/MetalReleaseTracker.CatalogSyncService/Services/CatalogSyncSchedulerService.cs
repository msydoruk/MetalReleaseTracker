using Hangfire;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;

namespace MetalReleaseTracker.CatalogSyncService.Services
{
    public class CatalogSyncSchedulerService : BackgroundService
    {
        private const string AlbumProcessingJobId = "AlbumProcessingJob";
        private const string AlbumProcessedPublisherJobId = "AlbumProcessedPublisherJob";
        private readonly IRecurringJobManager _recurringJobManager;

        public CatalogSyncSchedulerService(
            IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _recurringJobManager.AddOrUpdate<AlbumProcessingJob>(
                AlbumProcessingJobId,
                job => job.RunProcessingJob(cancellationToken),
                Cron.Daily);

            _recurringJobManager.AddOrUpdate<AlbumProcessedPublisherJob>(
                AlbumProcessedPublisherJobId,
                job => job.RunPublisherJob(cancellationToken),
                Cron.Daily);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
