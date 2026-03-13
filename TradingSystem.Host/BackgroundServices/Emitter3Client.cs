using System.Threading.Channels;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Normalizers;
using TradingSystem.Infrastructure.WebSocket;

namespace TradingSystem.Host.BackgroundServices;

/// <summary>
/// WebSocket клиент источника 3.
/// Ожидаемый формат JSON: {"s", "p", "v", "t"}
/// </summary>
public sealed class Emitter3Client(
    Emitter3Normalizer normalizer,
    Channel<NormalizedTickDto> channel,
    ILogger<Emitter3Client> logger,
    IConfiguration configuration) : WebSocketClientBase(
        new Uri(configuration["Emitters:Emitter3:Url"] ?? "ws://localhost:8083"),
        normalizer,
        channel,
        logger)
{

    /// <inheritdoc/>
    protected override string GetEmitterName() => "Emitter3";
}
