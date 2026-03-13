using TradingSystem.Infrastructure.Normalizers;

namespace TradingSystem.Tests.Unit.Normalizers;

public class Emitter2NormalizerTests
{
    private readonly Emitter2Normalizer _normalizer = new();

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Valid_Json()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""volume"":0.10,""timestamp"":""2025-03-12T10:30:00Z""}";
        var expectedTimestamp = DateTimeOffset.Parse("2025-03-12T10:30:00Z");

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Emitter2", result.Exchange);
        Assert.Equal("ETHUSD", result.Ticker);
        Assert.Equal(3500.50m, result.Price);
        Assert.Equal(0.10m, result.Volume);
        Assert.Equal(expectedTimestamp, result.Timestamp);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Not_Valid_Json()
    {
        // Arrange
        var raw = @"{not valid json}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Last_Price()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":-100,""volume"":0.10,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Zero_Last_Price()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":0,""volume"":0.10,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Invalid_Timestamp_Format()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""volume"":0.10,""timestamp"":""invalid""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Missing_Required_Field()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Negative_Volume()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""volume"":-0.10,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Zero_Volume()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""volume"":0,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Volume);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_Null_When_Empty_Ticker()
    {
        // Arrange
        var raw = @"{""ticker"":"""",""lastPrice"":3500.50,""volume"":0.10,""timestamp"":""2025-03-12T10:30:00Z""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NormalizeAsync_Should_Return_NormalizedTick_When_Unknown_Fields()
    {
        // Arrange
        var raw = @"{""ticker"":""ETHUSD"",""lastPrice"":3500.50,""volume"":0.10,""timestamp"":""2025-03-12T10:30:00Z"",""extra"":""value""}";

        // Act
        var result = await _normalizer.NormalizeAsync(raw, "Emitter2", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ETHUSD", result.Ticker);
    }
}
