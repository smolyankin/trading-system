using TradingSystem.Core.Models;

namespace TradingSystem.Core.Interfaces;

/// <summary>
/// Интерфейс нормализации тиков JSON
/// </summary>
public interface INormalizer
{
    /// <summary>
    /// Нормализация JSON от источника.
    /// </summary>
    /// <param name="raw">JSON.</param>
    /// <param name="source">Источник.</param>
    /// <param name="ct">Токен.</param>
    /// <returns>Нормализованный тик.</returns>
    Task<NormalizedTickDto?> NormalizeAsync(string raw, string source, CancellationToken ct);
}
