using System.Threading.Channels;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Normalizers;
using TradingSystem.Infrastructure.WebSocket;

namespace TradingSystem.Host.BackgroundServices;

/// <summary>
/// WebSocket клиент источника 2.
/// Ожидаемый формат JSON: {"ticker", "lastPrice", "volume", "timestamp"}
/// </summary>
public sealed class Emitter2Client(
    Emitter2Normalizer normalizer,
    Channel<NormalizedTickDto> channel,
    ILogger<Emitter2Client> logger,
    IConfiguration configuration) : WebSocketClientBase(
        new Uri(configuration["Emitters:Emitter2:Url"] ?? "ws://localhost:8082"),
        normalizer,
        channel,
        logger)
{

    /// <inheritdoc/>
    protected override string GetEmitterName() => "Emitter2";
}
