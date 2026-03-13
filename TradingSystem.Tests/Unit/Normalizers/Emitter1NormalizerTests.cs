using TradingSystem.Infrastructure.Normalizers;

namespace TradingSystem.Tests.Unit.Normalizers;

public class Emitter1NormalizerTests
{
    private readonly Emitter1Normalizer _normalizer = new();

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Valid_Json()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":0.01,""time"":1623456789000}";
        var expectedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(1623456789000);

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Emitter1", result.Exchange);
        Assert.Equal("BTCUSDT", result.Ticker);
        Assert.Equal(50000.12m, result.Price);
        Assert.Equal(0.01m, result.Volume);
        Assert.Equal(expectedTimestamp, result.Timestamp);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Missing_Required_Field()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":0.01}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Negative_Price()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":-100,""quantity"":0.01,""time"":1623456789000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Zero_Price()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":0,""quantity"":0.01,""time"":1623456789000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Timestamp()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":0.01,""time"":-1}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Not_Valid_Json()
    {
        // Arrange
        var raw = @"{not valid json}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Unknown_Fields()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":0.01,""time"":1623456789000,""extra"":123}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTCUSDT", result.Ticker);
        Assert.Equal(50000.12m, result.Price);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Negative_Volume()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":-0.01,""time"":1623456789000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Zero_Volume()
    {
        // Arrange
        var raw = @"{""symbol"":""BTCUSDT"",""price"":50000.12,""quantity"":0,""time"":1623456789000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Volume);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Empty_Symbol()
    {
        // Arrange
        var raw = @"{""symbol"":"""",""price"":50000.12,""quantity"":0.01,""time"":1623456789000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter1", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
