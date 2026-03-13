using TradingSystem.Core.Models;

namespace TradingSystem.Core.Entities;

public class Tick
{
    public long Id { get; set; }
    public string Exchange { get; set; } = null!;
    public string Ticker { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    
    public static Tick FromNormalizedTick(NormalizedTickDto normalizedTick) => new()
    {
        Exchange = normalizedTick.Exchange,
        Ticker = normalizedTick.Ticker,
        Price = normalizedTick.Price,
        Volume = normalizedTick.Volume,
        Timestamp = normalizedTick.Timestamp,
        ReceivedAt = normalizedTick.ReceivedAt
    };
}
