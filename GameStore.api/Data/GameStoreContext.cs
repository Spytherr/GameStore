using System;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(g => g.Price)
                .HasPrecision(18, 2);

            entity.Property(g => g.DiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(g => g.Description)
                .HasMaxLength(1000);

            entity.Property(g => g.ImageUrl)
                .HasMaxLength(500);

            entity.Property(g => g.Stock)
                .HasDefaultValue(0);
        });

        base.OnModelCreating(modelBuilder);
    }
}
