using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;

public class ParsingProgressTracker : IParsingProgressTracker
{
    private readonly ConcurrentDictionary<Guid, ParsingRunState> _activeRuns = new();
    private readonly ConcurrentDictionary<Guid, Channel<ParsingProgressEvent>> _subscribers = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ParsingProgressTracker> _logger;

    public ParsingProgressTracker(
        IServiceScopeFactory scopeFactory,
        ILogger<ParsingProgressTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void StartRun(Guid runId, ParsingJobType jobType, DistributorCode code, int totalItems)
    {
        var state = new ParsingRunState
        {
            RunId = runId,
            JobType = jobType,
            DistributorCode = code,
            TotalItems = totalItems,
            StartedAt = DateTime.UtcNow,
        };

        _activeRuns[runId] = state;
        PersistRunAsync(runId, state).FireAndForget(_logger);

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Started,
            runId,
            jobType,
            code,
            0,
            totalItems,
            0,
            null,
            $"Started parsing {totalItems} items",
            DateTime.UtcNow));
    }

    public void ItemProcessed(Guid runId, string itemDescription)
    {
        if (!_activeRuns.TryGetValue(runId, out var state))
        {
            return;
        }

        state.IncrementProcessed();

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Progress,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            state.TotalItems,
            state.FailedItems,
            itemDescription,
            null,
            DateTime.UtcNow));
    }

    public void ItemFailed(Guid runId, string itemDescription, string error)
    {
        if (!_activeRuns.TryGetValue(runId, out var state))
        {
            return;
        }

        state.IncrementFailed();

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Error,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            state.TotalItems,
            state.FailedItems,
            itemDescription,
            error,
            DateTime.UtcNow));
    }

    public void CompleteRun(Guid runId)
    {
        if (!_activeRuns.TryRemove(runId, out var state))
        {
            return;
        }

        UpdateRunInDbAsync(runId, ParsingRunStatus.Completed, state.ProcessedItems, state.FailedItems, null).FireAndForget(_logger);

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Completed,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            state.TotalItems,
            state.FailedItems,
            null,
            "Completed",
            DateTime.UtcNow));
    }

    public void FailRun(Guid runId, string errorMessage)
    {
        if (!_activeRuns.TryRemove(runId, out var state))
        {
            return;
        }

        UpdateRunInDbAsync(runId, ParsingRunStatus.Failed, state.ProcessedItems, state.FailedItems, errorMessage).FireAndForget(_logger);

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Completed,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            state.TotalItems,
            state.FailedItems,
            null,
            errorMessage,
            DateTime.UtcNow));
    }

    public async IAsyncEnumerable<ParsingProgressEvent> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var subscriberId = Guid.NewGuid();
        var channel = Channel.CreateBounded<ParsingProgressEvent>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });

        _subscribers[subscriberId] = channel;

        try
        {
            await foreach (var progressEvent in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return progressEvent;
            }
        }
        finally
        {
            _subscribers.TryRemove(subscriberId, out _);
        }
    }

    private void Broadcast(ParsingProgressEvent progressEvent)
    {
        foreach (var subscriber in _subscribers)
        {
            if (!subscriber.Value.Writer.TryWrite(progressEvent))
            {
                _logger.LogWarning("Dropped parsing progress event for subscriber {SubscriberId}.", subscriber.Key);
            }
        }
    }

    private async Task PersistRunAsync(Guid runId, ParsingRunState state)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParserServiceDbContext>();

        context.ParsingRuns.Add(new ParsingRunEntity
        {
            Id = runId,
            JobType = state.JobType,
            DistributorCode = state.DistributorCode,
            Status = ParsingRunStatus.Running,
            TotalItems = state.TotalItems,
            ProcessedItems = 0,
            FailedItems = 0,
            StartedAt = state.StartedAt,
        });

        await context.SaveChangesAsync();
    }

    private async Task UpdateRunInDbAsync(
        Guid runId,
        ParsingRunStatus status,
        int processedItems,
        int failedItems,
        string? errorMessage)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParserServiceDbContext>();

        var entity = await context.ParsingRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (entity == null)
        {
            return;
        }

        entity.Status = status;
        entity.ProcessedItems = processedItems;
        entity.FailedItems = failedItems;
        entity.CompletedAt = DateTime.UtcNow;
        entity.ErrorMessage = errorMessage?.Length > 2000 ? errorMessage[..2000] : errorMessage;

        await context.SaveChangesAsync();
    }

    private class ParsingRunState
    {
        private int _processedItems;
        private int _failedItems;

        public Guid RunId { get; set; }

        public ParsingJobType JobType { get; set; }

        public DistributorCode DistributorCode { get; set; }

        public int TotalItems { get; set; }

        public int ProcessedItems => _processedItems;

        public int FailedItems => _failedItems;

        public DateTime StartedAt { get; set; }

        public int IncrementProcessed() => Interlocked.Increment(ref _processedItems);

        public int IncrementFailed() => Interlocked.Increment(ref _failedItems);
    }
}

internal static class TaskExtensions
{
    public static async void FireAndForget(this Task task, ILogger logger)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Fire-and-forget task failed.");
        }
    }
}
