using System.Text.Json;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Infrastructure.Normalizers;

/// <summary>
/// Нормализация JSON из источника 2.
/// JSON: {"ticker", "lastPrice", "volume", "timestamp"}
/// </summary>
public sealed class Emitter2Normalizer : INormalizer
{
    /// <inheritdoc/>
    public Task<NormalizedTickDto?> NormalizeAsync(string raw, string source, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Task.FromResult<NormalizedTickDto?>(null);
        }

        try
        {
            using var document = JsonDocument.Parse(raw, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            var root = document.RootElement;

            var ticker = root.GetProperty("ticker").GetString();
            var lastPrice = root.GetProperty("lastPrice").GetDecimal();
            var volume = root.GetProperty("volume").GetDecimal();
            var timestampStr = root.GetProperty("timestamp").GetString();

            if (string.IsNullOrWhiteSpace(ticker)
                || lastPrice <= 0
                || volume < 0
                || !DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                return Task.FromResult<NormalizedTickDto?>(null);
            }

            return Task.FromResult<NormalizedTickDto?>(new NormalizedTickDto(
                source,
                ticker,
                lastPrice,
                volume,
                timestamp,
                DateTimeOffset.UtcNow
            ));
        }
        catch (JsonException)
        {
            return Task.FromResult<NormalizedTickDto?>(null);
        }
        catch (KeyNotFoundException)
        {
            return Task.FromResult<NormalizedTickDto?>(null);
        }
    }
}
