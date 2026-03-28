using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class NewsPublisherService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsPublisherService> _logger;

    public NewsPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<NewsPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NewsPublisherService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishScheduledArticlesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error publishing scheduled news articles");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task PublishScheduledArticlesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreDataServiceDbContext>();

        var articlesToPublish = await context.NewsArticles
            .Where(article => !article.IsPublished
                && article.ScheduledPublishDate != null
                && article.ScheduledPublishDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (articlesToPublish.Count == 0)
        {
            return;
        }

        foreach (var article in articlesToPublish)
        {
            article.IsPublished = true;
            article.ScheduledPublishDate = null;
            article.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Published {Count} scheduled news articles", articlesToPublish.Count);
    }
}
