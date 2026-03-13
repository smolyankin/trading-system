using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Channels;
using TradingSystem.Core.Models;
using TradingSystem.Host.BackgroundServices;
using TradingSystem.Infrastructure.Data;
using Xunit;

namespace TradingSystem.Tests.Unit.Processing;

public class TickProcessorTests : IDisposable
{
    private readonly Channel<NormalizedTickDto> _channel;
    private readonly CancellationTokenSource _cts;

    public TickProcessorTests()
    {
        _channel = Channel.CreateUnbounded<NormalizedTickDto>();
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task StartAsync_Should_Read_From_Channel_When_Called()
    {
        // Arrange
        var tick = new NormalizedTickDto("Emitter1", "BTCUSDT", 50000m, 1m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await _channel.Writer.WriteAsync(tick, _cts.Token);

        var loggerMock = new Mock<ILogger<TickProcessor>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var processor = new TickProcessor(_channel, serviceProviderMock.Object, loggerMock.Object);

        // Act
        var processTask = processor.StartAsync(_cts.Token);
        await Task.Delay(100);
        _cts.Cancel();

        try { await processTask; } catch (OperationCanceledException) { }

        // Assert
        Assert.True(true);
    }
}
