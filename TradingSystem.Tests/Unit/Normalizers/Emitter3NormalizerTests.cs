using TradingSystem.Infrastructure.Normalizers;

namespace TradingSystem.Tests.Unit.Normalizers;

public class Emitter3NormalizerTests
{
    private readonly Emitter3Normalizer _normalizer = new();

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Valid_Json()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":5.0,""t"":1741795200000}";
        var expectedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(1741795200000);

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Emitter3", result.Exchange);
        Assert.Equal("SOLUSDT", result.Ticker);
        Assert.Equal(150.25m, result.Price);
        Assert.Equal(5.0m, result.Volume);
        Assert.Equal(expectedTimestamp, result.Timestamp);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Missing_Required_Field()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":5.0}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Unknown_Fields()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":5.0,""t"":1741795200000,""extra"":""ignored""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SOLUSDT", result.Ticker);
        Assert.Equal(150.25m, result.Price);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Zero_Volume()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":0,""t"":1741795200000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Volume);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Negative_Price()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":-100,""v"":5.0,""t"":1741795200000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Zero_Price()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":0,""v"":5.0,""t"":1741795200000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Timestamp()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":5.0,""t"":-1}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Not_Valid_Json()
    {
        // Arrange
        var raw = @"{not valid json}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Negative_Volume()
    {
        // Arrange
        var raw = @"{""s"":""SOLUSDT"",""p"":150.25,""v"":-5.0,""t"":1741795200000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Empty_Symbol()
    {
        // Arrange
        var raw = @"{""s"":"""",""p"":150.25,""v"":5.0,""t"":1741795200000}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter3", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
