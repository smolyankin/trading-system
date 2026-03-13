using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSystem.Core.Entities;

namespace TradingSystem.Infrastructure.Data.Configurations;

public class TickConfiguration : IEntityTypeConfiguration<Tick>
{
    public void Configure(EntityTypeBuilder<Tick> entity)
    {
        entity.ToTable("ticks");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Exchange)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Ticker)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Price)
            .HasPrecision(18, 8)
            .IsRequired();

        entity.Property(e => e.Volume)
            .HasPrecision(18, 8)
            .IsRequired();

        entity.Property(e => e.Timestamp)
            .IsRequired();

        entity.Property(e => e.ReceivedAt)
            .IsRequired();
    }
}
