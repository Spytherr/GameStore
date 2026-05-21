using System;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>()
            .Property(g => g.Price)
            .HasPrecision(18, 2);
        
        base.OnModelCreating(modelBuilder);
    }
}
