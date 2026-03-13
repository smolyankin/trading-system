namespace TradingSystem.Host.BackgroundServices;

public record DeduplicationKey(
    string Exchange,
    string Ticker,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    long SecondTimestamp
);
