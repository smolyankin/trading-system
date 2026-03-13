namespace TradingSystem.Core.Models;

public record NormalizedTickDto(
    string Exchange,
    string Ticker,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    DateTimeOffset ReceivedAt
);
