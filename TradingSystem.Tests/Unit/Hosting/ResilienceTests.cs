using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Moq;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.WebSocket;

namespace TradingSystem.Tests.Unit.Hosting;

public class ResilienceTests
{
    [Fact]
    public void WebSocketClientBase_Should_Not_Crash_When_WebSocket_Exception()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WebSocketClientBase>>();
        var normalizerMock = new Mock<INormalizer>();
        var channel = Channel.CreateUnbounded<NormalizedTickDto>();
        var uri = new Uri("ws://localhost:9999");

        // Act
        var exception = Record.Exception(() =>
            new TestWebSocketClient(uri, normalizerMock.Object, channel, loggerMock.Object)
        );

        // Assert
        Assert.Null(exception);
    }

    private class TestWebSocketClient : WebSocketClientBase
    {
        public TestWebSocketClient(
            Uri uri,
            INormalizer normalizer,
            Channel<NormalizedTickDto> channel,
            ILogger<WebSocketClientBase> logger)
            : base(uri, normalizer, channel, logger)
        {
        }

        protected override string GetEmitterName() => "TestEmitter";
    }
}
