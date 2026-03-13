using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using TradingSystem.Core.Models;
using TradingSystem.Host.BackgroundServices;
using TradingSystem.Infrastructure.Data;
using TradingSystem.Infrastructure.Normalizers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TradingSystemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(Channel.CreateUnbounded<NormalizedTickDto>());

builder.Services.AddSingleton<Emitter1Normalizer>();
builder.Services.AddSingleton<Emitter2Normalizer>();
builder.Services.AddSingleton<Emitter3Normalizer>();

builder.Services.AddHostedService<Emitter1Client>();
builder.Services.AddHostedService<Emitter2Client>();
builder.Services.AddHostedService<Emitter3Client>();

builder.Services.AddHostedService<TickProcessor>();

builder.Services.AddOpenApi();

var app = builder.Build();

var skipDbInit = app.Configuration.GetValue<bool>("SkipDatabaseInitialization");
if (!skipDbInit)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TradingSystemDbContext>();
        try
        {
            db.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to apply database migrations");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/stats", async (TradingSystemDbContext db) =>
{
    try
    {
        var count = await db.Ticks.CountAsync();
        return Results.Ok(count);
    }
    catch (Exception)
    {
        return Results.StatusCode(503);
    }
})
.WithName("GetStats");

app.Run();

public partial class Program { }
