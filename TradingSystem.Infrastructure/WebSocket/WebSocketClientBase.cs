using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Infrastructure.WebSocket;

/// <summary>
/// Абстрактный WebSocket клиент для тиков.
/// </summary>
public abstract class WebSocketClientBase : BackgroundService
{
    private readonly ILogger<WebSocketClientBase> _logger;
    private readonly INormalizer _normalizer;
    private readonly Channel<NormalizedTickDto> _channel;
    private readonly Uri _uri;

    protected WebSocketClientBase(
        Uri uri,
        INormalizer normalizer,
        Channel<NormalizedTickDto> channel,
        ILogger<WebSocketClientBase> logger)
    {
        _uri = uri ?? throw new ArgumentNullException(nameof(uri));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int retryCount = 0;
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Connecting to {Uri}", _uri);

                using var client = new ClientWebSocket();
                await client.ConnectAsync(_uri, stoppingToken);

                _logger.LogInformation("Connected to {Uri}", _uri);
                retryCount = 0;

                var buffer = new byte[4096];
                var messageBuffer = new List<byte>();

                while (client.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
                        break;
                    }

                    messageBuffer.AddRange(buffer.Take(result.Count));

                    if (result.EndOfMessage)
                    {
                        var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        await ProcessMessageAsync(message, stoppingToken);
                        messageBuffer.Clear();
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "WebSocket connection to {Uri} failed", _uri);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error connecting to {Uri}", _uri);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                var delay = CalculateBackoff(retryCount, random);
                _logger.LogInformation("Retrying connection to {Uri} in {Delay}ms", _uri, delay);
                await Task.Delay(delay, stoppingToken);
                retryCount++;
            }
        }

        _logger.LogInformation("WebSocket client for {Uri} stopped", _uri);
    }

    /// <summary>
    /// Рассчитать экспоненциальную задержку.
    /// </summary>
    private static int CalculateBackoff(int retryCount, Random random)
    {
        var baseDelay = Math.Min(1000 * Math.Pow(2, retryCount), 30000);
        var jitter = random.Next(0, 500);
        return (int)baseDelay + jitter;
    }

    /// <summary>
    /// Обработка сообщения.
    /// </summary>
    private async Task ProcessMessageAsync(string message, CancellationToken ct)
    {
        var normalized = await _normalizer.NormalizeAsync(message, GetEmitterName(), ct);

        if (normalized is not null)
        {
            await _channel.Writer.WriteAsync(normalized, ct);
        }
        else
        {
            _logger.LogDebug("Failed to normalize message from {Emitter}", GetEmitterName());
        }
    }

    /// <summary>
    /// Получить имя источника.
    /// </summary>
    protected abstract string GetEmitterName();
}
