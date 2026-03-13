using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Infrastructure.Data;
using TradingSystem.Infrastructure.Normalizers;

namespace TradingSystem.Tests.Integration;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SkipDatabaseInitialization", "true" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TradingSystemDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TradingSystemDbContext>(options =>
            {
                options.UseInMemoryDatabase($"IntegrationTest_{Guid.NewGuid()}");
            });

            if (services.All(d => d.ServiceType != typeof(Channel<NormalizedTickDto>)))
            {
                services.AddSingleton(Channel.CreateUnbounded<NormalizedTickDto>());
            }

            services.AddSingleton<INormalizer, Emitter1Normalizer>();
            services.AddSingleton<INormalizer, Emitter2Normalizer>();
            services.AddSingleton<INormalizer, Emitter3Normalizer>();

            var loggingDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILoggerProvider) &&
                     d.ImplementationType?.Name.Contains("EventLog") == true);

            if (loggingDescriptor != null)
            {
                services.Remove(loggingDescriptor);
            }
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });
    }
}
