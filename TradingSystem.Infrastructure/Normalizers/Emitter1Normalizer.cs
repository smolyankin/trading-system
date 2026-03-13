using System.Text.Json;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Infrastructure.Normalizers;

/// <summary>
/// Нормализация JSON из источника 1.
/// JSON: {"symbol", "price", "quantity", "time"}
/// </summary>
public sealed class Emitter1Normalizer : INormalizer
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

            var symbol = root.GetProperty("symbol").GetString();
            var price = root.GetProperty("price").GetDecimal();
            var quantity = root.GetProperty("quantity").GetDecimal();
            var timeMs = root.GetProperty("time").GetInt64();

            if (string.IsNullOrWhiteSpace(symbol)
                || price <= 0
                || quantity < 0
                || timeMs < 0)
            {
                return Task.FromResult<NormalizedTickDto?>(null);
            }

            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timeMs);

            return Task.FromResult<NormalizedTickDto?>(new NormalizedTickDto(
                source,
                symbol,
                price,
                quantity,
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
