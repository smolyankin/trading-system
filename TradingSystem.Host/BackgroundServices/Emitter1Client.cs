using System.Threading.Channels;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Normalizers;
using TradingSystem.Infrastructure.WebSocket;

namespace TradingSystem.Host.BackgroundServices;

/// <summary>
/// WebSocket клиент источника 1.
/// Ожидаемый формат JSON: {"symbol", "price", "quantity", "time"}
/// </summary>
public sealed class Emitter1Client(
    Emitter1Normalizer normalizer,
    Channel<NormalizedTickDto> channel,
    ILogger<Emitter1Client> logger,
    IConfiguration configuration) : WebSocketClientBase(
        new Uri(configuration["Emitters:Emitter1:Url"] ?? "ws://localhost:8081"),
        normalizer,
        channel,
        logger)
{

    /// <inheritdoc/>
    protected override string GetEmitterName() => "Emitter1";
}
