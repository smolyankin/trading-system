using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Threading.Channels;
using TradingSystem.Core.Entities;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Data;
using TradingSystem.Infrastructure.Normalizers;
using Xunit;

namespace TradingSystem.Tests.Integration;

public class AggregatorFlowTests
{
    [Fact]
    public async Task Normalization_Should_Produce_Identical_Schema_When_All_Formats()
    {
        // Arrange
        var normalizer1 = new Emitter1Normalizer();
        var normalizer2 = new Emitter2Normalizer();
        var normalizer3 = new Emitter3Normalizer();

        var now = DateTimeOffset.UtcNow;
        var unixTimeMs = now.ToUnixTimeMilliseconds();
        var iso8601 = now.ToString("o");

        // Act
        var format1Msg = $"{{\"symbol\":\"BTCUSDT\",\"price\":50000.12,\"quantity\":0.01,\"time\":{unixTimeMs}}}";
        var format2Msg = $"{{\"ticker\":\"ETHUSD\",\"lastPrice\":3500.50,\"volume\":0.10,\"timestamp\":\"{iso8601}\"}}";
        var format3Msg = $"{{\"s\":\"SOLUSDT\",\"p\":150.25,\"v\":5.0,\"t\":{unixTimeMs}}}";

        var tick1 = await normalizer1.NormalizeAsync(format1Msg, "Emitter1", default);
        var tick2 = await normalizer2.NormalizeAsync(format2Msg, "Emitter2", default);
        var tick3 = await normalizer3.NormalizeAsync(format3Msg, "Emitter3", default);

        // Assert
        Assert.NotNull(tick1);
        Assert.NotNull(tick2);
        Assert.NotNull(tick3);

        Assert.Equal("Emitter1", tick1.Exchange);
        Assert.Equal("BTCUSDT", tick1.Ticker);
        Assert.Equal(50000.12m, tick1.Price);
        Assert.Equal(0.01m, tick1.Volume);

        Assert.Equal("Emitter2", tick2.Exchange);
        Assert.Equal("ETHUSD", tick2.Ticker);
        Assert.Equal(3500.50m, tick2.Price);
        Assert.Equal(0.10m, tick2.Volume);

        Assert.Equal("Emitter3", tick3.Exchange);
        Assert.Equal("SOLUSDT", tick3.Ticker);
        Assert.Equal(150.25m, tick3.Price);
        Assert.Equal(5.0m, tick3.Volume);

        var dbTick1 = Tick.FromNormalizedTick(tick1);
        var dbTick2 = Tick.FromNormalizedTick(tick2);
        var dbTick3 = Tick.FromNormalizedTick(tick3);

        Assert.Equal(typeof(Tick), dbTick1.GetType());
        Assert.Equal(typeof(Tick), dbTick2.GetType());
        Assert.Equal(typeof(Tick), dbTick3.GetType());
    }

    [Fact]
    public void TickEntity_From_NormalizedTick_Should_Work_When_Conversion()
    {
        // Arrange
        var normalizedTick = new NormalizedTickDto(
            Exchange: "TestEmitter",
            Ticker: "BTCUSDT",
            Price: 50000.12m,
            Volume: 0.01m,
            Timestamp: DateTimeOffset.UtcNow,
            ReceivedAt: DateTimeOffset.UtcNow
        );

        // Act
        var tick = Tick.FromNormalizedTick(normalizedTick);

        // Assert
        Assert.Equal("TestEmitter", tick.Exchange);
        Assert.Equal("BTCUSDT", tick.Ticker);
        Assert.Equal(50000.12m, tick.Price);
        Assert.Equal(0.01m, tick.Volume);
    }

    [Fact]
    public async Task Channel_Should_Work_When_Producer_Consumer_Flow()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<NormalizedTickDto>();
        var testTick = new NormalizedTickDto(
            Exchange: "TestEmitter",
            Ticker: "BTCUSDT",
            Price: 50000m,
            Volume: 1.0m,
            Timestamp: DateTimeOffset.UtcNow,
            ReceivedAt: DateTimeOffset.UtcNow
        );

        // Act
        await channel.Writer.WriteAsync(testTick);

        var readTick = await channel.Reader.ReadAsync();

        // Assert
        Assert.NotNull(readTick);
        Assert.Equal("TestEmitter", readTick.Exchange);
    }
}
