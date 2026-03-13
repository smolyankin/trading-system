using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TradingSystem.Emulator;

/// <summary>
/// Генерация тиков в трех разных форматах JSON.
/// </summary>
public class TickGenerator
{
    private readonly int _format;
    private readonly int _ticksPerSecond;
    private readonly Random _random = new();

    private static readonly string[] s_pairs = new[]
    {
        "BTCUSDT", "ETHUSDT", "SOLUSDT", "BNBUSDT", "ADAUSDT",
        "XRPUSDT", "DOGEUSDT", "DOTUSDT", "MATICUSDT", "LTCUSDT"
    };

    public TickGenerator(int format, int ticksPerSecond, ILogger logger)
    {
        if (format < 1 || format > 3)
        {
            throw new ArgumentException("Format must be 1, 2, or 3", nameof(format));
        }

        if (ticksPerSecond < 1 || ticksPerSecond > 1000)
        {
            throw new ArgumentException("Ticks per second must be between 1 and 1000", nameof(ticksPerSecond));
        }

        _format = format;
        _ticksPerSecond = ticksPerSecond;
    }

    /// <summary>
    /// Генерация тика.
    /// </summary>
    public async Task<string> GenerateTickAsync(CancellationToken ct)
    {
        // Wait for the appropriate delay to maintain tick rate
        var delay = 1000 / _ticksPerSecond;
        await Task.Delay(delay, ct);

        // Generate tick data
        var pair = s_pairs[_random.Next(s_pairs.Length)];
        var price = GetRandomPrice(pair);
        var volume = GetRandomVolume();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Return in configured format
        return _format switch
        {
            1 => GenerateFormat1(pair, price, volume, timestamp),
            2 => GenerateFormat2(pair, price, volume, timestamp),
            3 => GenerateFormat3(pair, price, volume, timestamp),
            _ => throw new InvalidOperationException($"Unknown format: {_format}")
        };
    }

    /// <summary>
    /// Генерация тика в формате 1: {"symbol", "price", "quantity", "time"}
    /// Пример: {"symbol":"BTCUSDT","price":50000.12,"quantity":0.01,"time":1623456789000}
    /// </summary>
    private static string GenerateFormat1(string symbol, decimal price, decimal quantity, long time)
    {
        var tick = new
        {
            symbol = symbol,
            price = Math.Round(price, 2),
            quantity = Math.Round(quantity, 4),
            time = time
        };

        return JsonSerializer.Serialize(tick);
    }

    /// <summary>
    /// Генерация тика в формате 2: {"ticker", "lastPrice", "volume", "timestamp"}
    /// Пример: {"ticker":"ETHUSD","lastPrice":3500.50,"volume":0.10,"timestamp":"2025-03-12T10:30:00Z"}
    /// </summary>
    private static string GenerateFormat2(string ticker, decimal lastPrice, decimal volume, long time)
    {
        var tick = new
        {
            ticker = ticker,
            lastPrice = Math.Round(lastPrice, 2),
            volume = Math.Round(volume, 4),
            timestamp = DateTimeOffset.FromUnixTimeMilliseconds(time).ToString("o")
        };

        return JsonSerializer.Serialize(tick);
    }

    /// <summary>
    /// Генерация тика в формате 3: {"s", "p", "v", "t"}
    /// Пример: {"s":"SOLUSDT","p":150.25,"v":5.0,"t":1741795200000}
    /// </summary>
    private static string GenerateFormat3(string s, decimal p, decimal v, long t)
    {
        var tick = new
        {
            s = s,
            p = Math.Round(p, 2),
            v = Math.Round(v, 4),
            t = t
        };

        return JsonSerializer.Serialize(tick);
    }

    /// <summary>
    /// Получить случайную цену.
    /// </summary>
    private decimal GetRandomPrice(string pair)
    {
        var basePrice = pair switch
        {
            "BTCUSDT" => 50000m,
            "ETHUSDT" => 3500m,
            "SOLUSDT" => 150m,
            "BNBUSDT" => 400m,
            "ADAUSDT" => 0.5m,
            "XRPUSDT" => 0.6m,
            "DOGEUSDT" => 0.15m,
            "DOTUSDT" => 8m,
            "MATICUSDT" => 0.9m,
            "LTCUSDT" => 80m,
            _ => 100m
        };

        var variation = (decimal)_random.NextDouble() * 0.04m - 0.02m;
        return basePrice * (1 + variation);
    }

    /// <summary>
    /// Получить случайный объем.
    /// </summary>
    private decimal GetRandomVolume()
    {
        return (decimal)_random.NextDouble() * 9.999m + 0.001m;
    }
}
