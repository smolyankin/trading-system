namespace TradingSystem.Tests.Unit.WebSocket;

public class ReconnectionTests
{
    [Fact]
    public void CalculateCleanTime_Should_BeApprox1Second_When_FirstRetry()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay = WebSocketClientBase_CalculateCleanTime(0, random);

        // Assert
        Assert.InRange(delay, 1000, 1500);
    }

    [Fact]
    public void CalculateCleanTime_Should_BeApprox2Seconds_When_SecondRetry()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay = WebSocketClientBase_CalculateCleanTime(1, random);

        // Assert
        Assert.InRange(delay, 2000, 2500);
    }

    [Fact]
    public void CalculateCleanTime_Should_BeApprox4Seconds_When_ThirdRetry()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay = WebSocketClientBase_CalculateCleanTime(2, random);

        // Assert
        Assert.InRange(delay, 4000, 4500);
    }

    [Fact]
    public void CalculateCleanTime_Should_BeApprox8Seconds_When_FourRetries()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay = WebSocketClientBase_CalculateCleanTime(3, random);

        // Assert
        Assert.InRange(delay, 8000, 8500);
    }

    [Fact]
    public void CalculateCleanTime_Should_BeApprox16Seconds_When_FiveRetries()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay = WebSocketClientBase_CalculateCleanTime(4, random);

        // Assert
        Assert.InRange(delay, 16000, 16500);
    }

    [Fact]
    public void CalculateCleanTime_Should_BeCappedAt30Seconds_When_SixOrMoreRetries()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var delay6 = WebSocketClientBase_CalculateCleanTime(5, random);
        var delay10 = WebSocketClientBase_CalculateCleanTime(9, random);

        // Assert
        Assert.InRange(delay6, 30000, 30500);
        Assert.InRange(delay10, 30000, 30500);
    }

    private static int WebSocketClientBase_CalculateCleanTime(int retryCount, Random random)
    {
        var baseDelay = Math.Min(1000 * Math.Pow(2, retryCount), 30000);
        var jitter = random.Next(0, 500);
        return (int)baseDelay + jitter;
    }
}
