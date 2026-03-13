using Microsoft.EntityFrameworkCore;
using TradingSystem.Core.Entities;
using TradingSystem.Infrastructure.Data.Configurations;

namespace TradingSystem.Infrastructure.Data;

public class TradingSystemDbContext : DbContext
{
    public DbSet<Tick> Ticks { get; set; } = null!;

    public TradingSystemDbContext(DbContextOptions<TradingSystemDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TickConfiguration).Assembly);
    }
}
