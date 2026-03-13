using TradingSystem.Core.Models;

namespace TradingSystem.Tests.Unit.Processing;

public class DeduplicationTests
{
    [Fact]
    public void IsDuplicate_Should_Return_True_When_Within_Same_Second()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.True(isDuplicate, "Ticks within 1 second with identical values should be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_Different_Ticker()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter1", "ETHUSD", 50000m, 1m, now, now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.False(isDuplicate, "Ticks with different tickers should not be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_Different_Price()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50001m, 1m, now, now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.False(isDuplicate, "Ticks with different prices should not be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_More_Than_One_Second_Apart()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, timestamp, timestamp);
        var tick2 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, timestamp, timestamp.AddMilliseconds(1500));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: false);

        // Assert
        Assert.False(isDuplicate, "Ticks more than 1 second apart should not be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_Different_Exchange()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter2", "BTCUSDT", 50000m, 1m, now, now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.False(isDuplicate, "Ticks from different exchanges should not be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_Different_Volume()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 2m, now, now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.False(isDuplicate, "Ticks with different volumes should not be duplicates");
    }

    [Fact]
    public void IsDuplicate_Should_Return_False_When_Identical_Values_But_Different_Timestamp()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tick1 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now, now);
        var tick2 = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, now.AddSeconds(1), now.AddMilliseconds(100));

        // Act
        var isDuplicate = AreTicksDuplicate(tick1, tick2, withinOneSecond: true);

        // Assert
        Assert.False(isDuplicate, "Ticks with different event timestamps should not be duplicates");
    }

    [Fact]
    public void SecondTimestamp_Should_Floor_Timestamp_To_Whole_Second_When_Called()
    {
        // Arrange
        var timestamp = DateTimeOffset.Parse("2025-03-12T10:30:45.789Z");
        var expectedBucket = DateTimeOffset.Parse("2025-03-12T10:30:45.000Z");

        // Act
        var bucket = GetSecondTimestamp(timestamp);

        // Assert
        Assert.Equal(expectedBucket, bucket);
        Assert.Equal(0, bucket.Millisecond);
        Assert.Equal(0, bucket.Ticks % 10_000_000);
    }

    private static bool AreTicksDuplicate(NormalizedTickDto tick1, NormalizedTickDto tick2, bool withinOneSecond)
    {
        if (tick1.Exchange != tick2.Exchange) return false;
        if (tick1.Ticker != tick2.Ticker) return false;
        if (tick1.Price != tick2.Price) return false;
        if (tick1.Volume != tick2.Volume) return false;
        if (tick1.Timestamp != tick2.Timestamp) return false;

        return withinOneSecond;
    }

    private static DateTimeOffset GetSecondTimestamp(DateTimeOffset timestamp)
    {
        return new DateTimeOffset(
            timestamp.Year,
            timestamp.Month,
            timestamp.Day,
            timestamp.Hour,
            timestamp.Minute,
            timestamp.Second,
            timestamp.Offset
        );
    }
}
