using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    public void StartRun(Guid runId, ParsingJobType jobType, DistributorCode code)
    {
        var state = new ParsingRunState
        {
            RunId = runId,
            JobType = jobType,
            DistributorCode = code,
            TotalItems = 0,
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
            0,
            0,
            null,
            "Started indexing",
            DateTime.UtcNow));
    }

    public void ItemProcessed(Guid runId, string itemDescription, params string[] categories)
    {
        if (!_activeRuns.TryGetValue(runId, out var state))
        {
            return;
        }

        state.IncrementProcessed();

        foreach (var category in categories)
        {
            state.IncrementCounter(category);
        }

        state.AddItem(new ParsingRunItemRecord(itemDescription, true, null, categories, DateTime.UtcNow));

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
            DateTime.UtcNow,
            state.GetCountersSnapshot()));
    }

    public void ItemFailed(Guid runId, string itemDescription, string error, params string[] categories)
    {
        if (!_activeRuns.TryGetValue(runId, out var state))
        {
            return;
        }

        state.IncrementFailed();

        foreach (var category in categories)
        {
            state.IncrementCounter(category);
        }

        state.AddItem(new ParsingRunItemRecord(itemDescription, false, error, categories, DateTime.UtcNow));

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
            DateTime.UtcNow,
            state.GetCountersSnapshot()));
    }

    public void CompleteRun(Guid runId)
    {
        if (!_activeRuns.TryRemove(runId, out var state))
        {
            return;
        }

        var finalTotal = state.TotalItems > 0 ? state.TotalItems : state.ProcessedItems;
        var counters = state.GetCountersSnapshot();
        var items = state.GetItems();

        UpdateRunInDbAsync(runId, ParsingRunStatus.Completed, state.ProcessedItems, state.FailedItems, null, counters, finalTotal).FireAndForget(_logger);
        PersistRunItemsAsync(runId, items).FireAndForget(_logger);

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Completed,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            finalTotal,
            state.FailedItems,
            null,
            "Completed",
            DateTime.UtcNow,
            counters));
    }

    public void FailRun(Guid runId, string errorMessage)
    {
        if (!_activeRuns.TryRemove(runId, out var state))
        {
            return;
        }

        var finalTotal = state.TotalItems > 0 ? state.TotalItems : state.ProcessedItems;
        var counters = state.GetCountersSnapshot();
        var items = state.GetItems();

        UpdateRunInDbAsync(runId, ParsingRunStatus.Failed, state.ProcessedItems, state.FailedItems, errorMessage, counters, finalTotal).FireAndForget(_logger);
        PersistRunItemsAsync(runId, items).FireAndForget(_logger);

        Broadcast(new ParsingProgressEvent(
            ParsingEventType.Completed,
            runId,
            state.JobType,
            state.DistributorCode,
            state.ProcessedItems,
            finalTotal,
            state.FailedItems,
            null,
            errorMessage,
            DateTime.UtcNow,
            counters));
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
        string? errorMessage,
        Dictionary<string, int>? counters,
        int totalItems)
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
        entity.TotalItems = totalItems;
        entity.CompletedAt = DateTime.UtcNow;
        entity.ErrorMessage = errorMessage?.Length > 2000 ? errorMessage[..2000] : errorMessage;

        if (counters != null && counters.Count > 0)
        {
            entity.CountersJson = JsonSerializer.Serialize(counters);
        }

        await context.SaveChangesAsync();
    }

    private async Task PersistRunItemsAsync(Guid runId, List<ParsingRunItemRecord> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParserServiceDbContext>();

        var entities = items.Select(item => new ParsingRunItemEntity
        {
            Id = Guid.NewGuid(),
            ParsingRunId = runId,
            ItemDescription = item.ItemDescription.Length > 500 ? item.ItemDescription[..500] : item.ItemDescription,
            IsSuccess = item.IsSuccess,
            ErrorMessage = item.ErrorMessage?.Length > 2000 ? item.ErrorMessage[..2000] : item.ErrorMessage,
            Categories = item.Categories,
            ProcessedAt = item.ProcessedAt,
        }).ToList();

        await context.ParsingRunItems.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    private class ParsingRunState
    {
        private readonly ConcurrentDictionary<string, int> _counters = new();
        private readonly List<ParsingRunItemRecord> _items = [];
        private readonly object _itemsLock = new();
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

        public void IncrementCounter(string name)
        {
            _counters.AddOrUpdate(name, 1, (_, current) => current + 1);
        }

        public Dictionary<string, int> GetCountersSnapshot()
        {
            return new Dictionary<string, int>(_counters);
        }

        public void AddItem(ParsingRunItemRecord item)
        {
            lock (_itemsLock)
            {
                _items.Add(item);
            }
        }

        public List<ParsingRunItemRecord> GetItems()
        {
            lock (_itemsLock)
            {
                return new List<ParsingRunItemRecord>(_items);
            }
        }
    }
}

internal record ParsingRunItemRecord(
    string ItemDescription,
    bool IsSuccess,
    string? ErrorMessage,
    string[] Categories,
    DateTime ProcessedAt);

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
