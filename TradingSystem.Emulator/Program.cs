using Microsoft.Extensions.Logging;
using TradingSystem.Emulator;

var formatStr = Environment.GetEnvironmentVariable("EMITTER_FORMAT") ?? "1";
var portStr = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var ticksPerSecondStr = Environment.GetEnvironmentVariable("TICKS_PER_SECOND") ?? "25";

if (!int.TryParse(formatStr, out int format) || format < 1 || format > 3)
{
    Console.Error.WriteLine("EMITTER_FORMAT must be 1, 2, or 3");
    Environment.Exit(1);
}

if (!int.TryParse(portStr, out int port) || port < 1 || port > 65535)
{
    Console.Error.WriteLine("PORT must be a valid port number (1-65535)");
    Environment.Exit(1);
}

if (!int.TryParse(ticksPerSecondStr, out int ticksPerSecond) || ticksPerSecond < 1 || ticksPerSecond > 1000)
{
    Console.Error.WriteLine("TICKS_PER_SECOND must be between 1 and 1000");
    Environment.Exit(1);
}

if (ticksPerSecond < 15 || ticksPerSecond > 35)
{
    Console.WriteLine($"Warning: TICKS_PER_SECOND ({ticksPerSecond}) is outside recommended range (15-35)");
}

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("TradingSystem.Emulator");

var url = $"http://*:{port}/";

logger.LogInformation("Starting WebSocket Emulator");
logger.LogInformation("Format: {Format} ({Description})", format, GetFormatDescription(format));
logger.LogInformation("Port: {Port}", port);
logger.LogInformation("Ticks per second: {TicksPerSecond}", ticksPerSecond);

var tickGenerator = new TickGenerator(format, ticksPerSecond, logger);
var server = new WebSocketServer(logger);
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    logger.LogInformation("Shutdown signal received");
    server.Stop();
    cts.Cancel();
};

try
{
    await server.StartAsync(url, tickGenerator.GenerateTickAsync);
}
catch (OperationCanceledException)
{
    // Normal shutdown
}
catch (Exception ex)
{
    logger.LogError(ex, "Server error");
    Environment.Exit(1);
}

logger.LogInformation("WebSocket Emulator stopped");

static string GetFormatDescription(int format)
{
    return format switch
    {
        1 => "Standard Format: {symbol, price, quantity, time}",
        2 => "Extended Format: {ticker, lastPrice, volume, timestamp}",
        3 => "Compact Format: {s, p, v, t}",
        _ => "Unknown"
    };
}
