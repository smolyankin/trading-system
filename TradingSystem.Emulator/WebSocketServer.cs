using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using System.Net;
using System.Buffers;

namespace TradingSystem.Emulator;

public class WebSocketServer
{
    private readonly ConcurrentBag<WebSocket> _clients = [];
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;

    public WebSocketServer(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(string url, Func<CancellationToken, Task<string>> tickGenerator)
    {
        _logger.LogInformation("WebSocket emulator starting on {Url}", url);

        var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        _logger.LogInformation("WebSocket emulator listening on {Url}", url);

        var acceptTask = Task.Run(() => AcceptConnectionsAsync(listener, tickGenerator, _cts.Token));

        await Task.Delay(Timeout.Infinite, _cts.Token);
    }

    public void Stop()
    {
        _logger.LogInformation("WebSocket emulator stopping");
        _cts.Cancel();
    }

    private async Task AcceptConnectionsAsync(HttpListener listener, Func<CancellationToken, Task<string>> tickGenerator, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && listener.IsListening)
        {
            try
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _logger.LogInformation("New WebSocket connection from {RemoteEndPoint}", context.Request.RemoteEndPoint);

                    var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                    var webSocket = wsContext.WebSocket;

                    _clients.Add(webSocket);

                    _ = Task.Run(() => SendTicksAsync(webSocket, tickGenerator, ct), ct);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!ct.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error accepting WebSocket connection");
                }
            }
        }
    }

    private async Task SendTicksAsync(WebSocket webSocket, Func<CancellationToken, Task<string>> tickGenerator, CancellationToken ct)
    {
        // Use ArrayPool for better memory efficiency with frequent allocations
        var pool = ArrayPool<byte>.Shared;

        try
        {
            while (!ct.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                byte[]? buffer = null;
                try
                {
                    // Generate tick
                    var tickJson = await tickGenerator(ct);

                    // Rent buffer from pool instead of allocating new array each time
                    buffer = pool.Rent(Encoding.UTF8.GetMaxByteCount(tickJson.Length));
                    var byteCount = Encoding.UTF8.GetBytes(tickJson, 0, tickJson.Length, buffer, 0);

                    // Send to client using sliced ArraySegment (no additional allocation)
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, byteCount),
                        WebSocketMessageType.Text,
                        true,
                        ct);
                }
                catch (WebSocketException)
                {
                    // Client disconnected
                    break;
                }
                finally
                {
                    // Return buffer to pool to avoid memory leaks
                    if (buffer != null)
                    {
                        pool.Return(buffer);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ticks to WebSocket client");
        }
        finally
        {
            _ = _clients.TryTake(out webSocket);
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch
            {
                // Ignore close errors
            }
            _logger.LogInformation("WebSocket client disconnected");
        }
    }
}
