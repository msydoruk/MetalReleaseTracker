using Hangfire;

using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.BackgroundServices.Settings;

using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.BackgroundServices.Workers
{
    public class AlbumSynchronizationWorker : BackgroundService
    {
        private readonly ILogger<AlbumSynchronizationWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly string _cronExpression;

        public AlbumSynchronizationWorker(
             ILogger<AlbumSynchronizationWorker> logger,
             IServiceScopeFactory serviceScopeFactory,
             IRecurringJobManager recurringJobManager,
             IOptions<AlbumSynchronizationSettings> albumSynchronizationSettings
            )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _recurringJobManager = recurringJobManager;
            _cronExpression = albumSynchronizationSettings.Value.CronExpression;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlbumSynchronizationWorker is starting.");

            try
            {
                _recurringJobManager.AddOrUpdate("SynchronizeAllAlbumsJob", () => RunSynchronizationJob(), _cronExpression);

                _logger.LogInformation("SynchronizeAllAlbums task scheduled to run based on the provided Cron expression.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to schedule the SynchronizeAllAlbums task.");
            }

            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlbumSynchronizationWorker stopped.");
            await base.StopAsync(cancellationToken);
        }

        public async Task RunSynchronizationJob()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var albumSynchronizationService = scope.ServiceProvider.GetRequiredService<IAlbumSynchronizationService>();

                try
                {
                    await albumSynchronizationService.SynchronizeAllAlbums();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error occurred while synchronizing albums.");
                }
            }
        }
    }
}