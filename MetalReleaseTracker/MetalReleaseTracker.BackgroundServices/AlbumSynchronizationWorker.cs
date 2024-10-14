using Hangfire;
using MetalReleaseTracker.Application.Interfaces;

namespace MetalReleaseTracker.BackgroundServices
{
    public class AlbumSynchronizationWorker : BackgroundService
    {
        private readonly ILogger<AlbumSynchronizationWorker> _logger;
        private readonly IAlbumSynchronizationService _albumSynchronizationService;
        private readonly IRecurringJobManager _recurringJobManager;

        public AlbumSynchronizationWorker(
             ILogger<AlbumSynchronizationWorker> logger,
             IAlbumSynchronizationService albumSynchronizationService,
             IRecurringJobManager recurringJobManager
            )
        {
            _logger = logger;
            _albumSynchronizationService = albumSynchronizationService;
            _recurringJobManager = recurringJobManager;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlbumSynchronizationWorker is starting.");

            _recurringJobManager.AddOrUpdate("SynchronizeAllAlbumsJob", () => _albumSynchronizationService.SynchronizeAllAlbums(), Cron.Daily);                               

            _logger.LogInformation("SynchronizeAllAlbums task scheduled to run daily.");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"The background service is working: {DateTimeOffset.Now}");
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlbumSynchronizationWorker stopped.");
            return base.StopAsync(cancellationToken);
        }
    }
}
