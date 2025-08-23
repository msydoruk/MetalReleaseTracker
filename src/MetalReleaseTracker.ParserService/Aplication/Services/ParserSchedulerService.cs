using Hangfire;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Aplication.Services
{
    public class ParserSchedulerService : BackgroundService
    {
        private const string AlbumParsedPublisherJobId = "AlbumParsedPublisherJob";
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly List<ParserDataSource> _parserDataSources;

        public ParserSchedulerService(
            IRecurringJobManager recurringJobManager,
            IOptions<List<ParserDataSource>> parserDataSources)
        {
            _recurringJobManager = recurringJobManager;
            _parserDataSources = parserDataSources.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var parserDataSource in _parserDataSources)
            {
                _recurringJobManager.AddOrUpdate<AlbumParsingJob>(
                    parserDataSource.Name,
                    job => job.RunParserJob(parserDataSource, cancellationToken),
                    Cron.Daily);
            }

            _recurringJobManager.AddOrUpdate<AlbumParsedPublisherJob>(
                AlbumParsedPublisherJobId,
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
