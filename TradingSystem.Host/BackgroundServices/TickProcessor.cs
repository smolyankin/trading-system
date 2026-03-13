using System.Collections.Concurrent;
using System.Threading.Channels;
using TradingSystem.Core.Entities;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Data;

namespace TradingSystem.Host.BackgroundServices;

/// <summary>
/// Фоновая задача по сохранению тиков в базе данных из канала.
/// </summary>
public sealed class TickProcessor : BackgroundService
{
    private readonly Channel<NormalizedTickDto> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TickProcessor> _logger;
    private readonly ConcurrentDictionary<DeduplicationKey, DateTimeOffset> _deduplicationCache;
    private readonly CancellationTokenSource _cts = new();
    private const int BatchSize = 100;
    private const int BatchTimeoutMs = 50;
    private const int DeduplicationWindowSeconds = 1;
    private const int CleanupIntervalMs = 2000;
    private const int MaxCacheSize = 5000;

    public TickProcessor(
        Channel<NormalizedTickDto> channel,
        IServiceProvider serviceProvider,
        ILogger<TickProcessor> logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deduplicationCache = new ConcurrentDictionary<DeduplicationKey, DateTimeOffset>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TickProcessor starting");

        var cleanupTask = RunPeriodicCleanupAsync();

        var batch = new List<NormalizedTickDto>(BatchSize);
        var lastBatchTime = DateTimeOffset.UtcNow;
        var totalProcessed = 0L;
        var lastLogTime = DateTimeOffset.UtcNow;

        try
        {
            await foreach (var tick in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (IsDuplicate(tick))
                {
                    _logger.LogDebug("Remove duplicate tick: {Exchange} {Ticker} {Price} {Volume} @ {Timestamp}",
                        tick.Exchange, tick.Ticker, tick.Price, tick.Volume, tick.Timestamp);
                    continue;
                }

                batch.Add(tick);
                totalProcessed++;

                var shouldProcessBatch = batch.Count >= BatchSize ||
                    (DateTimeOffset.UtcNow - lastBatchTime).TotalMilliseconds >= BatchTimeoutMs;

                if (shouldProcessBatch && batch.Count > 0)
                {
                    await ProcessBatchAsync(batch, stoppingToken);
                    batch.Clear();
                    lastBatchTime = DateTimeOffset.UtcNow;

                    if ((DateTimeOffset.UtcNow - lastLogTime).TotalSeconds >= 10)
                    {
                        var rate = totalProcessed / (DateTimeOffset.UtcNow - lastLogTime).TotalSeconds;
                        _logger.LogInformation("Processing rate: {Rate:F1} ticks/sec, Cache size: {CacheSize}",
                            rate, _deduplicationCache.Count);
                        totalProcessed = 0;
                        lastLogTime = DateTimeOffset.UtcNow;
                    }
                }
            }
        }
        finally
        {
            _logger.LogInformation("TickProcessor shutdown started. Ticks in batch: {Count}", batch.Count);

            if (batch.Count > 0)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await ProcessBatchAsync(batch, cts.Token);
                    _logger.LogInformation("Successfully saved {Count} ticks on shutdown", batch.Count);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Saved cancelled by timeout. Lost {Count} ticks)", batch.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save remaining ticks on shutdown. Lost {Count} ticks)", batch.Count);
                }
            }
            else
            {
                _logger.LogInformation("No remaining ticks to save on shutdown");
            }

            try
            {
                _cts.Cancel();
                await Task.WhenAny(cleanupTask, Task.Delay(TimeSpan.FromSeconds(2)));
                _logger.LogInformation("Cleanup task stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error stopping cleanup task");
            }
        }

        _logger.LogInformation("TickProcessor stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TickProcessor normal stopping");
        await base.StopAsync(cancellationToken);
        _cts.Dispose();
    }

    /// <summary>
    /// Периодическая очистка кеша дедубликации.
    /// </summary>
    private async Task RunPeriodicCleanupAsync()
    {
        _logger.LogInformation("Background cleanup task started (interval: {Interval}ms, max cache: {Max})",
            CleanupIntervalMs, MaxCacheSize);

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CleanupIntervalMs, _cts.Token);
                var beforeCount = _deduplicationCache.Count;
                CleanupOldEntries();
                var afterCount = _deduplicationCache.Count;

                _logger.LogDebug("Cache size: {Before} -> {After}", beforeCount, afterCount);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic cleanup");
            }
        }

        _logger.LogInformation("Background cleanup task stopped");
    }

    /// <summary>
    /// Проверка на дубли.
    /// </summary>
    private bool IsDuplicate(NormalizedTickDto tick)
    {
        var secondBucket = tick.Timestamp.ToUnixTimeSeconds();
        var key = new DeduplicationKey(
            tick.Exchange,
            tick.Ticker,
            tick.Price,
            tick.Volume,
            tick.Timestamp,
            secondBucket
        );

        var isNew = _deduplicationCache.TryAdd(key, tick.ReceivedAt);

        return !isNew;
    }

    /// <summary>
    /// Очистка старых данных из кеша.
    /// </summary>
    private void CleanupOldEntries()
    {
        var currentCount = _deduplicationCache.Count;

        if (currentCount > MaxCacheSize)
        {
            _logger.LogWarning("Cache size {Count} exceeded maximum {Max}, clearing cache",
                currentCount, MaxCacheSize);
            _deduplicationCache.Clear();
            return;
        }

        var cleanTime = DateTimeOffset.UtcNow.AddSeconds(-DeduplicationWindowSeconds);
        var removedCount = 0;

        var keysToRemove = new List<DeduplicationKey>(currentCount / 10);

        foreach (var kvp in _deduplicationCache)
        {
            if (kvp.Value < cleanTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (_deduplicationCache.TryRemove(key, out _))
            {
                removedCount++;
            }
        }

        if (removedCount > 0 || currentCount > 1000)
        {
            _logger.LogDebug("Cleaned up {Count} old entries from deduplication cache (remaining: {Remaining})",
                removedCount, _deduplicationCache.Count);
        }
    }

    /// <summary>
    /// Сохранение тиков пачками.
    /// </summary>
    private async Task ProcessBatchAsync(List<NormalizedTickDto> batch, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TradingSystemDbContext>();

        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

        var entities = batch.Select(Tick.FromNormalizedTick).ToList();
        dbContext.AddRange(entities);

        try
        {
            await dbContext.SaveChangesAsync(ct);
            _logger.LogDebug("Processed batch of {Count} ticks", batch.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Batch processing cancelled during shutdown. Lost {Count} ticks)", batch.Count);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save batch of {Count} ticks", batch.Count);
            throw;
        }
    }
}
