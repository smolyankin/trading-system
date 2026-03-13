using System.Text.Json;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Infrastructure.Normalizers;

/// <summary>
/// Нормализация JSON из источника 1.
/// JSON: JSON: {"s", "p", "v", "t"}
/// </summary>
public sealed class Emitter3Normalizer : INormalizer
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

            var symbol = root.GetProperty("s").GetString();
            var price = root.GetProperty("p").GetDecimal();
            var volume = root.GetProperty("v").GetDecimal();
            var timeMs = root.GetProperty("t").GetInt64();

            if (string.IsNullOrWhiteSpace(symbol)
                || price <= 0
                || volume < 0
                || timeMs < 0)
            {
                return Task.FromResult<NormalizedTickDto?>(null);
            }

            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timeMs);

            return Task.FromResult<NormalizedTickDto?>(new NormalizedTickDto(
                source,
                symbol,
                price,
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
        catch (ArgumentException)
        {
            return Task.FromResult<NormalizedTickDto?>(null);
        }
    }
}
